using System.Security.Cryptography;
using System.Text;

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
        Task<Token> RefreshToken(RefreshTokenCommand tokenRefresh);
        Task<TwoFactorChallenge> SignIn(SignInCommand credentials);
        Task<Token> VerifyTwoFactor(VerifyTwoFactorCommand command);
        Task SignOut(string jwt);
        Task SendPasswordChangePinAsync(string email);
        Task ChangePasswordWithPinAsync(string email, string pin, string newPassword);
    }

    public class AuthService : IAuthService {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IValidator<SignInCommand> _credentialsValidator;
        private readonly IValidator<RefreshTokenCommand> _tokenRefreshValidator;
        private readonly IValidator<VerifyTwoFactorCommand> _verifyTwoFactorValidator;
        private readonly IMemoryCache _invalidatedTokenCache;
        private readonly JwtSettings _jwtSettings;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService,
             IEmailService emailService, IValidator<SignInCommand> credentialsValidator, IValidator<RefreshTokenCommand> tokenRefreshValidator,
             IValidator<VerifyTwoFactorCommand> verifyTwoFactorValidator, IMemoryCache cache, IOptions<JwtSettings> jwtSettings) {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _credentialsValidator = credentialsValidator;
            _tokenRefreshValidator = tokenRefreshValidator;
            _verifyTwoFactorValidator = verifyTwoFactorValidator;
            _invalidatedTokenCache = cache;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Token> RefreshToken(RefreshTokenCommand tokenRefresh) {
            _tokenRefreshValidator.ValidateAndThrow(tokenRefresh);

            var principal = _tokenService.GetPrincipalFromToken(tokenRefresh.JWT);
            if (principal == null || principal.Identity?.Name != tokenRefresh.Email)
                throw new SecurityTokenException("Invalid email or token");

            var user = await _userManager.FindByEmailAsync(tokenRefresh.Email);
            if (user == null || user.RefreshToken != tokenRefresh.RefreshToken || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
                throw new SecurityTokenException("Invalid email or token");

            var newToken = _tokenService.GenerateToken(user);
            user.RefreshToken = newToken.RefreshToken;
            user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiryInMinutes);
            await _userManager.UpdateAsync(user);

            var expiration = TimeSpan.FromMinutes(_jwtSettings.TokenExpiryInMinutes);
            _invalidatedTokenCache.Set(tokenRefresh.JWT, true, expiration);

            return newToken;
        }

        public async Task<TwoFactorChallenge> SignIn(SignInCommand credentials) {
            _credentialsValidator.ValidateAndThrow(credentials);
            var result = await _signInManager.PasswordSignInAsync(credentials.Email, credentials.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded) {
                var user = await _userManager.FindByEmailAsync(credentials.Email);

                var pin = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
                user!.TwoFactorPin = ComputePinHash(pin);
                user.TwoFactorPinExpiry = DateTimeOffset.UtcNow.AddMinutes(5);
                await _userManager.UpdateAsync(user);

                await _emailService.SendEmailAsync(
                    credentials.Email,
                    "Your verification code",
                    $"Your verification code is: {pin}\n\nThis code expires in 5 minutes.");

                return new TwoFactorChallenge { Email = credentials.Email };
            }
            throw new SignInFailedException("Invalid login attempt");
        }

        public async Task<Token> VerifyTwoFactor(VerifyTwoFactorCommand command) {
            _verifyTwoFactorValidator.ValidateAndThrow(command);

            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
                throw new SignInFailedException("Invalid verification attempt");

            var pinHash = ComputePinHash(command.Pin);
            var storedHash = user.TwoFactorPin ?? string.Empty;
            var hashesMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(pinHash),
                Encoding.UTF8.GetBytes(storedHash));

            if (!hashesMatch || user.TwoFactorPinExpiry <= DateTimeOffset.UtcNow)
                throw new SignInFailedException("Invalid or expired verification code");

            user.TwoFactorPin = null;
            user.TwoFactorPinExpiry = DateTimeOffset.MinValue;

            var token = _tokenService.GenerateToken(user);
            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiryInMinutes);
            await _userManager.UpdateAsync(user);

            return token;
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

        private string ComputePinHash(string pin) {
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var pinBytes = Encoding.UTF8.GetBytes(pin);
            return Convert.ToHexString(HMACSHA256.HashData(keyBytes, pinBytes));
        }

        public async Task SendPasswordChangePinAsync(string email) {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new SignInFailedException("User not found");

            var pin = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.TwoFactorPin = ComputePinHash(pin);
            user.TwoFactorPinExpiry = DateTimeOffset.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            await _emailService.SendEmailAsync(
                email,
                "Password Change Verification Code",
                $"Your verification code is: {pin}\n\nThis code expires in 5 minutes.");
        }

        public async Task ChangePasswordWithPinAsync(string email, string pin, string newPassword) {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new SignInFailedException("User not found");

            var pinHash = ComputePinHash(pin);
            var storedHash = user.TwoFactorPin ?? string.Empty;
            var hashesMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(pinHash),
                Encoding.UTF8.GetBytes(storedHash));

            if (!hashesMatch || user.TwoFactorPinExpiry <= DateTimeOffset.UtcNow)
                throw new SignInFailedException("Invalid or expired verification code");

            user.TwoFactorPin = null;
            user.TwoFactorPinExpiry = DateTimeOffset.MinValue;
            await _userManager.UpdateAsync(user);

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                throw new InvalidOperationException("Failed to remove current password.");

            var addResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addResult.Succeeded) {
                var errors = string.Join(" ", addResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
        }
    }
}
