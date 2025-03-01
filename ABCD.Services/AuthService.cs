using ABCD.Lib;
using ABCD.Lib.Auth;
using ABCD.Lib.Exceptions;

using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ABCD.Services {
    public interface IAuthService {
        Task<Token> SignIn(SignInCredentials credentials);
        Task SignOut(string jwt);
    }

    public class AuthService : IAuthService {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IValidator<SignInCredentials> _credentialsValidator;
        private readonly IMemoryCache _invalidatedTokenCache;
        private readonly JwtSettings _jwtSettings;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService,
             IValidator<SignInCredentials> credentialsValidator, IMemoryCache cache, IOptions<JwtSettings> jwtSettings) {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _credentialsValidator = credentialsValidator;
            _invalidatedTokenCache = cache;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Token> SignIn(SignInCredentials credentials) {
            _credentialsValidator.ValidateAndThrow(credentials);
            var result = await _signInManager.PasswordSignInAsync(credentials.Email, credentials.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded) {
                var user = await _userManager.FindByEmailAsync(credentials.Email);
                var token = _tokenService.GenerateToken(user);
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
