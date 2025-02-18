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
            _cacheMock = new Mock<IMemoryCache>();

            var jwtSettings = new JwtSettings {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SecretKey = "test-secret-key",
                TokenExpiryInMinutes = 60
            };
            _jwtSettingsMock.Setup(s => s.Value).Returns(jwtSettings);

            _authService = new AuthService(
                _userManagerMock.Object, _signInManagerMock.Object, _jwtSettingsMock.Object,
                _tokenHandlerMock.Object, _userRegistrationValidatorMock.Object, _userLoginValidatorMock.Object, _cacheMock.Object);
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
        }

        [Fact]
        public void InvalidateToken_ValidToken_SetsCache() {
            // Arrange
            var token = "test-token";
            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            // Act
            _authService.InvalidateToken(token);

            // Assert
            _cacheMock.Verify(m => m.CreateEntry(token), Times.Once);
        }
    }
}
