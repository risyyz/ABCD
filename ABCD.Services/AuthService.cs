using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using ABCD.Lib;
using ABCD.Lib.Auth;
using ABCD.Lib.Exceptions;

using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ABCD.Services {
    public interface IAuthService {
        Task RegisterUser(UserRegistration userRegistration);
        Task<(string Token, string RefreshToken)> LoginUser(UserLogin userLogin);
        Task InvalidateToken(string token);
        Task<(string Token, string RefreshToken)> RefreshToken(string token, string refreshToken);
    }

    public class AuthService : IAuthService {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly JsonWebTokenHandler _tokenHandler;
        private readonly IValidator<UserRegistration> _userRegistrationValidator;
        private readonly IValidator<UserLogin> _userLoginValidator;
        private readonly IMemoryCache _invalidatedTokenCache;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
            IOptions<JwtSettings> jwtSettings, JsonWebTokenHandler tokenHandler, IValidator<UserRegistration> userRegistrationValidator,
            IValidator<UserLogin> userLoginValidator, IMemoryCache cache) {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _tokenHandler = tokenHandler;
            _userRegistrationValidator = userRegistrationValidator;
            _userLoginValidator = userLoginValidator;
            _invalidatedTokenCache = cache;
        }

        public async Task RegisterUser(UserRegistration userRegistration) {
            _userRegistrationValidator.ValidateAndThrow(userRegistration);
            var user = new ApplicationUser { UserName = userRegistration.Email, Email = userRegistration.Email };
            await _userManager.CreateAsync(user, userRegistration.Password);
        }

        public async Task<(string Token, string RefreshToken)> LoginUser(UserLogin userLogin) {
            _userLoginValidator.ValidateAndThrow(userLogin);
            var result = await _signInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded) {
                var user = await _userManager.FindByEmailAsync(userLogin.Email);
                var token = GenerateToken(user);
                var refreshToken = GenerateNewRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiryInMinutes);
                await _userManager.UpdateAsync(user);
                return (token, refreshToken);
            }
            throw new LoginFailedException("Invalid login attempt");
        }

        public async Task<(string Token, string RefreshToken)> RefreshToken(string token, string refreshToken) {
            var user = await GetUserFromToken(token);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow) {
                throw new SecurityTokenException("Invalid or expired refresh token");
            }

            var newToken = GenerateToken(user);
            var newRefreshToken = GenerateNewRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiryInMinutes);
            await _userManager.UpdateAsync(user);
            return (newToken, newRefreshToken);
        }

        private async Task<ApplicationUser?> GetUserFromToken(string token) {
            var principal = GetPrincipalFromExpiredToken(token);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            return await _userManager.FindByEmailAsync(email);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _jwtSettings.GetTokenValidationParameters(), out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private string GenerateToken(ApplicationUser user) {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.NormalizedUserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Admin") })
            };

            return _tokenHandler.CreateToken(tokenDescriptor);
        }

        private string GenerateNewRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public async Task InvalidateToken(string token) {
            if (!string.IsNullOrWhiteSpace(token)) {
                var user = await GetUserFromToken(token);
                if (!string.IsNullOrWhiteSpace(user?.RefreshToken)) {
                    user.RefreshToken = string.Empty;
                    user.RefreshTokenExpiryTime = DateTimeOffset.MinValue;
                    await _userManager.UpdateAsync(user);
                }
                
                var expiration = TimeSpan.FromMinutes(_jwtSettings.TokenExpiryInMinutes);
                _invalidatedTokenCache.Set(token, true, expiration);
            }
        }
    }
}
