using System.Security.Claims;
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
        Task<string> LoginUser(UserLogin userLogin);
        Task InvalidateToken(string token);
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

        public async Task<string> LoginUser(UserLogin userLogin) {
            _userLoginValidator.ValidateAndThrow(userLogin);
            var result = await _signInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded) {
                var user = await _userManager.FindByEmailAsync(userLogin.Email);
                return await GenerateToken(user);
            }
            throw new LoginFailedException("Invalid login attempt");
        }

        private async Task<string> GenerateToken(ApplicationUser user) {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddMinutes(_jwtSettings.ExpiryInMinutes),
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.NormalizedUserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Admin") })
            };

            return _tokenHandler.CreateToken(tokenDescriptor);
        }

        public Task InvalidateToken(string token) {
            if (!string.IsNullOrWhiteSpace(token)) {
                var expiration = TimeSpan.FromMinutes(_jwtSettings.ExpiryInMinutes);
                _invalidatedTokenCache.Set(token, true, expiration);
            }
            return Task.CompletedTask;
        }
    }
}
