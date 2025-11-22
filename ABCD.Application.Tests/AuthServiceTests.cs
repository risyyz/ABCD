using System.Security.Claims;
using System.Text;

using ABCD.Lib;
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

namespace ABCD.Application.Tests {
    public class AuthServiceTests {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly Mock<IValidator<SignInCredentials>> _userLoginValidatorMock;
        private readonly Mock<IValidator<TokenRefreshment>> _tokenRefreshValidatorMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly AuthService _authService;

        public AuthServiceTests() {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            _tokenServiceMock = new Mock<ITokenService>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _userLoginValidatorMock = new Mock<IValidator<SignInCredentials>>();
            _tokenRefreshValidatorMock = new Mock<IValidator<TokenRefreshment>>();
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
                _userLoginValidatorMock.Object, _tokenRefreshValidatorMock.Object, _cacheMock.Object, _jwtSettingsMock.Object);
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
            await act.Should().ThrowAsync<SignInFailedException>().WithMessage("*Invalid login attempt*");
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

        [Fact]
        public async Task SignOut_NullClaimsPrincipal_DoesNotSetCache() {
            // Arrange
            var jwt = "valid_jwt_token";

            _tokenServiceMock.Setup(t => t.GetPrincipalFromToken(jwt)).Returns((ClaimsPrincipal)null);

            // Act
            await _authService.SignOut(jwt);

            // Assert
            _userManagerMock.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
            _cacheMock.Verify(m => m.CreateEntry(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignOut_UserNotFound_DoesNotSetCache() {
            // Arrange
            var jwt = "valid_jwt_token";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "user@example.com")
            }));

            _tokenServiceMock.Setup(t => t.GetPrincipalFromToken(jwt)).Returns(claimsPrincipal);
            _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync((ApplicationUser)null);

            // Act
            await _authService.SignOut(jwt);

            // Assert
            _userManagerMock.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
            _cacheMock.Verify(m => m.CreateEntry(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignOut_ValidToken_SetsCache() {
            // Arrange
            var jwt = "valid_jwt_token";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "user@example.com")
            }));
            var user = new ApplicationUser {
                Email = "user@example.com",
                RefreshToken = "refresh_token",
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            _tokenServiceMock.Setup(t => t.GetPrincipalFromToken(jwt)).Returns(claimsPrincipal);
            _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            // Act
            await _authService.SignOut(jwt);

            // Assert
            _userManagerMock.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _cacheMock.Verify(m => m.CreateEntry(jwt), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_InvalidInputParameters_ThrowsArgumentException() {
            // Arrange
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Email", "Invalid email format") });
            _tokenRefreshValidatorMock.Setup(v => v.Validate(It.IsAny<IValidationContext>()))
                .Throws(new ValidationException(validationResult.Errors));

            var tokenRefresh = new TokenRefreshment {
                Email = null,
                JWT = null,
                RefreshToken = null
            };

            // Act
            Func<Task> act = async () => await _authService.RefreshToken(tokenRefresh);

            // Assert
            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Invalid email format*"); ;
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_ThrowsSecurityTokenException() {
            // Arrange
            var tokenRefresh = new TokenRefreshment {
                Email = "user@example.com",
                JWT = "invalid-jwt",
                RefreshToken = "valid-refresh-token"
            };

            _tokenServiceMock.Setup(x => x.GetPrincipalFromToken(It.IsAny<string>())).Returns((ClaimsPrincipal)null);

            // Act
            Func<Task> act = async () => await _authService.RefreshToken(tokenRefresh);

            // Assert
            await act.Should().ThrowAsync<SecurityTokenException>().WithMessage("Invalid token");
        }

        [Fact]
        public async Task RefreshToken_NonMatchingEmail_ThrowsSecurityTokenException() {
            // Arrange
            var tokenRefresh = new TokenRefreshment {
                Email = "user@example.com",
                JWT = "invalid-jwt",
                RefreshToken = "valid-refresh-token"
            };

            _tokenServiceMock.Setup(x => x.GetPrincipalFromToken(It.IsAny<string>())).Returns(
                new ClaimsPrincipal(
                    new List<ClaimsIdentity> {
                        new ClaimsIdentity(new Claim[] {
                            new Claim(ClaimTypes.Name, "abcd@gmail.com")
                        })
                    }
            ));

            // Act
            Func<Task> act = async () => await _authService.RefreshToken(tokenRefresh);

            // Assert
            await act.Should().ThrowAsync<SecurityTokenException>().WithMessage("Invalid token");
        }

        [Fact]
        public async Task RefreshToken_NonMatchingRefreshToken_ThrowsSecurityTokenException() {
            // Arrange
            var tokenRefresh = new TokenRefreshment {
                Email = "user@example.com",
                JWT = "valid-jwt",
                RefreshToken = "invalid-refresh-token"
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "user@example.com") }));
            _tokenServiceMock.Setup(x => x.GetPrincipalFromToken(It.IsAny<string>())).Returns(principal);

            var user = new ApplicationUser {
                Email = "user@example.com",
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _authService.RefreshToken(tokenRefresh);

            // Assert
            await act.Should().ThrowAsync<SecurityTokenException>().WithMessage("Invalid refresh token");
        }

        [Fact]
        public async Task RefreshToken_ExpiredRefreshToken_ThrowsSecurityTokenException() {
            // Arrange
            var tokenRefresh = new TokenRefreshment {
                Email = "user@example.com",
                JWT = "valid-jwt",
                RefreshToken = "valid-refresh-token"
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "user@example.com") }));
            _tokenServiceMock.Setup(x => x.GetPrincipalFromToken(It.IsAny<string>())).Returns(principal);

            var user = new ApplicationUser {
                Email = "user@example.com",
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(-10) //expired 10 mins ago
            };
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _authService.RefreshToken(tokenRefresh);

            // Assert
            await act.Should().ThrowAsync<SecurityTokenException>().WithMessage("Invalid refresh token");
        }

        [Fact]
        public async Task RefreshToken_ValidInput_ReturnsNewToken() {
            // Arrange
            var tokenRefresh = new TokenRefreshment {
                Email = "user@example.com",
                JWT = "valid-jwt",
                RefreshToken = "valid-refresh-token"
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "user@example.com") }));
            _tokenServiceMock.Setup(x => x.GetPrincipalFromToken(It.IsAny<string>())).Returns(principal);

            var user = new ApplicationUser {
                Email = "user@example.com",
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var newToken = new Token {
                JWT = "new-jwt",
                RefreshToken = "new-refresh-token"
            };
            _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>())).Returns(newToken);

            var cacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
            // Act
            var result = await _authService.RefreshToken(tokenRefresh);

            // Assert
            result.Should().BeEquivalentTo(newToken);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _cacheMock.Verify(m => m.CreateEntry(tokenRefresh.JWT), Times.Once);
        }
    }
}