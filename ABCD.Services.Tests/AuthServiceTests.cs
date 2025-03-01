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
using Microsoft.IdentityModel.Tokens;

using Moq;

using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace ABCD.Services.Tests {
    public class AuthServiceTests {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly Mock<IValidator<UserRegistration>> _userRegistrationValidatorMock;
        private readonly Mock<IValidator<SignInCredentials>> _userLoginValidatorMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly AuthService _authService;

        public AuthServiceTests() {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            _tokenServiceMock = new Mock<ITokenService>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _userLoginValidatorMock = new Mock<IValidator<SignInCredentials>>();
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

            _authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _tokenServiceMock.Object, 
                _userLoginValidatorMock.Object, _cacheMock.Object, _jwtSettingsMock.Object);
        }

        [Fact]
        public async Task SignIn_InvalidRequest_ThrowsValidationException() {
            // Arrange
            var credentials = new SignInCredentials { Email = "invalid-email", Password = "password" };
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Email", "Invalid email format") });

            _userLoginValidatorMock
                .Setup(v => v.Validate(It.IsAny<IValidationContext>()))
                .Throws(new ValidationException(validationResult.Errors));

            // Act
            Func<Task> act = async () => await _authService.SignIn(credentials);

            // Assert
            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Invalid email format*");            
        }

        [Fact]
        public async Task SignIn_InvalidCredentials_ThrowsLoginFailedException() {
            // Arrange
            var credentials = new SignInCredentials { Email = "test@example.com", Password = "password" };

            _userLoginValidatorMock
                .Setup(v => v.Validate(It.IsAny<IValidationContext>()))
                .Returns(new ValidationResult());

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(credentials.Email, credentials.Password, false, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            Func<Task> act = async () => await _authService.SignIn(credentials);

            // Assert
            await act.Should().ThrowAsync<LoginFailedException>().WithMessage("*Invalid login attempt*");
            _userManagerMock.Verify(m => m.FindByEmailAsync(credentials.Email), Times.Never);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
            _tokenServiceMock.Verify(m => m.GenerateToken(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task SignIn_ValidCredentials_ReturnsToken() {
            // Arrange
            var credentials = new SignInCredentials { Email = "test@example.com", Password = "password" };
            var user = new ApplicationUser { Email = credentials.Email, UserName = credentials.Email, NormalizedUserName = credentials.Email };
            var token = new Token { JWT = "test-jwt-token", RefreshToken = "test-refresh-token" };

            _userLoginValidatorMock
                .Setup(v => v.Validate(It.IsAny<IValidationContext>()))
                .Returns(new ValidationResult());

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(credentials.Email, credentials.Password, false, false))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(m => m.FindByEmailAsync(credentials.Email)).ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns(token);

            // Act
            var result = await _authService.SignIn(credentials);

            // Assert
            result.JWT.Should().Be(token.JWT);
            result.RefreshToken.Should().Be(token.RefreshToken);
            _userManagerMock.Verify(m => m.FindByEmailAsync(credentials.Email), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _tokenServiceMock.Verify(m => m.GenerateToken(It.IsAny<ApplicationUser>()), Times.Once);
        }
    }
}