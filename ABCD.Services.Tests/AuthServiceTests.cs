using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ABCD.Lib;
using ABCD.Lib.Auth;
using ABCD.Lib.Exceptions;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using Moq;

namespace ABCD.Services.Tests {
    public class AuthServiceTests {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly Mock<JsonWebTokenHandler> _tokenHandlerMock;
        private readonly Mock<IValidator<UserRegistration>> _userRegistrationValidatorMock;
        private readonly Mock<IValidator<UserLogin>> _userLoginValidatorMock;
        private readonly Mock<ISecurityTokenValidator> _tokenValidatorMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly AuthService _authService;

        public AuthServiceTests() {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _tokenHandlerMock = new Mock<JsonWebTokenHandler>();
            _userRegistrationValidatorMock = new Mock<IValidator<UserRegistration>>();
            _userLoginValidatorMock = new Mock<IValidator<UserLogin>>();
            _tokenValidatorMock = new Mock<ISecurityTokenValidator>();
            _cacheMock = new Mock<IMemoryCache>();

            var jwtSettings = new JwtSettings {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SecretKey = "test-secret-key",
                TokenExpiryInMinutes = 60
            };
            _jwtSettingsMock.Setup(s => s.Value).Returns(jwtSettings);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            SecurityToken securityToken = new JwtSecurityToken(signingCredentials: credentials);
            _tokenValidatorMock.Setup(x => x.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out securityToken)).Returns(new ClaimsPrincipal());

            _authService = new AuthService(
                _userManagerMock.Object, _signInManagerMock.Object, _jwtSettingsMock.Object,
                _tokenHandlerMock.Object, _userRegistrationValidatorMock.Object, _userLoginValidatorMock.Object, _tokenValidatorMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task RegisterUser_InvalidRequest_ThrowsValidationException() {
            // Arrange
            var userRegistration = new UserRegistration { Email = "invalid-email", Password = "password", PasswordConfirmation = "password" };
            var validationResult = new ValidationResult(new List<FluentValidation.Results.ValidationFailure> { new FluentValidation.Results.ValidationFailure("Email", "Invalid email format") });

            _userRegistrationValidatorMock
                .Setup(v => v.Validate(It.IsAny<IValidationContext>()))
                .Throws(new ValidationException(validationResult.Errors));

            // Act & Assert
            var act = () => _authService.RegisterUser(userRegistration);
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task RegisterUser_ValidRequest_CreatesUser() {
            // Arrange
            var userRegistration = new UserRegistration { Email = "test@example.com", Password = "password", PasswordConfirmation = "password" };
            _userRegistrationValidatorMock
                .Setup(v => v.Validate(userRegistration))
                .Returns(new ValidationResult());

            // Act
            await _authService.RegisterUser(userRegistration);

            // Assert
            _userManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(u => u.Email == userRegistration.Email), userRegistration.Password), Times.Once);
        }

        [Fact]
        public async Task LoginUser_InvalidCredentials_ThrowsLoginFailedException() {
            // Arrange
            var userLogin = new UserLogin { Email = "test@example.com", Password = "password" };

            _userLoginValidatorMock
                .Setup(v => v.Validate(userLogin))
                .Returns(new ValidationResult());

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(userLogin.Email, userLogin.Password, false, false)).ReturnsAsync(SignInResult.Failed);

            // Act & Assert
            var act = () => _authService.LoginUser(userLogin);
            await act.Should().ThrowAsync<LoginFailedException>();
            _userManagerMock.Verify(m => m.FindByEmailAsync(userLogin.Email), Times.Never);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task LoginUser_ValidCredentials_ReturnsToken() {
            // Arrange
            var userLogin = new UserLogin { Email = "test@example.com", Password = "password" };
            var user = new ApplicationUser { Email = userLogin.Email, UserName = userLogin.Email, NormalizedUserName = userLogin.Email };
            var token = "test-token";

            _userLoginValidatorMock
                .Setup(v => v.Validate(userLogin))
                .Returns(new ValidationResult());

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(userLogin.Email, userLogin.Password, false, false)).ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(m => m.FindByEmailAsync(userLogin.Email)).ReturnsAsync(user);
            _tokenHandlerMock.Setup(t => t.CreateToken(It.IsAny<SecurityTokenDescriptor>())).Returns(token);

            // Act
            var result = await _authService.LoginUser(userLogin);

            // Assert
            result.Token.Should().Be(token);
            result.RefreshToken.Should().NotBeNullOrEmpty();
            _userManagerMock.Verify(m => m.FindByEmailAsync(userLogin.Email), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task InvalidateToken_InValidToken_DoesNotSetCache(string token) {
            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            // Act
            await _authService.InvalidateToken(token);

            // Assert
            _cacheMock.Verify(m => m.CreateEntry(It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task InvalidateToken_ValidToken_SetsCache() {
            // Arrange
            var token = "valid_token";
            var user = new ApplicationUser {
                RefreshToken = "refresh_token",
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            // Act
            await _authService.InvalidateToken(token);

            // Assert
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _cacheMock.Verify(m => m.CreateEntry(token), Times.Once);
        }
        
        //refresh token tests
    }
}