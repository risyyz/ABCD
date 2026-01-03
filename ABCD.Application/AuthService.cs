using ABCD.Application.Exceptions;
using ABCD.Infra.Data;
using ABCD.Lib;

using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ABCD.Application {
    public interface IAuthService {
        Task<Token> RefreshToken(TokenRefreshment tokenRefresh); 
        Task<Token> SignIn(SignInCredentials credentials);
        Task SignOut(string jwt);
    }

    public class AuthService : IAuthService {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IValidator<SignInCredentials> _credentialsValidator;
        private readonly IValidator<TokenRefreshment> _tokenRefreshValidator;
        private readonly IMemoryCache _invalidatedTokenCache;
        private readonly JwtSettings _jwtSettings;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService,
             IValidator<SignInCredentials> credentialsValidator, IValidator<TokenRefreshment> tokenRefreshValidator, IMemoryCache cache, IOptions<JwtSettings> jwtSettings) {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _credentialsValidator = credentialsValidator;
            _tokenRefreshValidator = tokenRefreshValidator;
            _invalidatedTokenCache = cache;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Token> RefreshToken(TokenRefreshment tokenRefresh) {
            _tokenRefreshValidator.ValidateAndThrow(tokenRefresh);

            var principal = _tokenService.GetPrincipalFromToken(tokenRefresh.JWT);
            if (principal == null || principal.Identity?.Name != tokenRefresh.Email)
                throw new SecurityTokenException("Invalid token");

            var user = await _userManager.FindByEmailAsync(tokenRefresh.Email);
            if (user == null || user.RefreshToken != tokenRefresh.RefreshToken || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
                throw new SecurityTokenException("Invalid refresh token");

            var newToken = _tokenService.GenerateToken(user);
            user.RefreshToken = newToken.RefreshToken;
            user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiryInMinutes);
            await _userManager.UpdateAsync(user);

            var expiration = TimeSpan.FromMinutes(_jwtSettings.TokenExpiryInMinutes);
            _invalidatedTokenCache.Set(tokenRefresh.JWT, true, expiration);

            return newToken;
        }

        public async Task<Token> SignIn(SignInCredentials credentials) {
            _credentialsValidator.ValidateAndThrow(credentials);
            var result = await _signInManager.PasswordSignInAsync(credentials.Email, credentials.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded) {
                var user = await _userManager.FindByEmailAsync(credentials.Email);
                var token = _tokenService.GenerateToken(user);
                user.RefreshToken = token.RefreshToken;
                user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiryInMinutes);
                await _userManager.UpdateAsync(user);
                return token;
            }
            throw new SignInFailedException("Invalid login attempt");
        }

        public async Task SignOut(string jwt) {
            if(string.IsNullOrWhiteSpace(jwt))
                return;

            var claims = _tokenService.GetPrincipalFromToken(jwt);
            if (claims == null)
                return;
            
            var user = await _userManager.FindByEmailAsync(claims.Identity.Name);
            if(user == null)
                return;

            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiryTime = DateTimeOffset.MinValue;
            await _userManager.UpdateAsync(user);

            var expiration = TimeSpan.FromMinutes(_jwtSettings.TokenExpiryInMinutes);
            _invalidatedTokenCache.Set(jwt, true, expiration);
        }
    }
}
