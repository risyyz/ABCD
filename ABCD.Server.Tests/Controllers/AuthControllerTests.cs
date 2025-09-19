using ABCD.Lib;
using ABCD.Lib.Exceptions;
using ABCD.Server.Controllers;
using ABCD.Server.Requests;
using ABCD.Services;

using FluentAssertions;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace ABCD.Server.Tests.Controllers {
    public class AuthControllerTests {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IClassMapper> _mapperMock;
        private readonly Mock<BearerTokenReader> _tokenReaderMock;
        private readonly AuthController _controller;

        public AuthControllerTests() {
            _authServiceMock = new Mock<IAuthService>();
            _mapperMock = new Mock<IClassMapper>();
            _tokenReaderMock = new Mock<BearerTokenReader>(Mock.Of<IHttpContextAccessor>());
            _controller = new AuthController(_authServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task SignIn_MissingRequiredParameters_ReturnsBadRequest() {
            // Arrange
            var request = new SignInRequest { Email = null, Password = null };
            var credentials = new SignInCredentials { Email = null, Password = null };
            string message = "Missing parameters";
            _mapperMock.Setup(m => m.Map<SignInRequest, SignInCredentials>(request)).Returns(credentials);
            _authServiceMock.Setup(s => s.SignIn(credentials)).ThrowsAsync(
                new ValidationException(new List<ValidationFailure> { new ValidationFailure { ErrorMessage = message } }));

            // Act
            var result = await _controller.SignIn(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be(message);
        }

        [Fact]
        public async Task SignIn_InvalidCredentials_ReturnsUnauthorized() {
            // Arrange
            var request = new SignInRequest { Email = "test@example.com", Password = "password" };
            var credentials = new SignInCredentials { Email = "test@example.com", Password = "password" };
            string message = "Invalid login attempt.";
            _mapperMock.Setup(m => m.Map<SignInRequest, SignInCredentials>(request)).Returns(credentials);
            _authServiceMock.Setup(s => s.SignIn(credentials)).ThrowsAsync(new SignInFailedException(message));

            // Act
            var result = await _controller.SignIn(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.Value.Should().Be(message);
        }

        [Fact]
        public async Task SignIn_ValidCredentials_ReturnsOk() {
            // Arrange
            var signInRequest = new SignInRequest { Email = "test@example.com", Password = "password" };
            var credentials = new SignInCredentials { Email = "test@example.com", Password = "password" };
            var jwt = "test-token";
            var refreshToken = "test-refresh-token";
            var token = new Token { JWT = jwt, RefreshToken = refreshToken };

            _mapperMock.Setup(m => m.Map<SignInRequest, SignInCredentials>(signInRequest)).Returns(credentials);
            _authServiceMock.Setup(s => s.SignIn(credentials)).ReturnsAsync(token);

            // Act
            var result = await _controller.SignIn(signInRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(new { success = true, token = jwt, refreshToken = refreshToken });
        }

        [Fact]
        public async Task SignOut_ValidToken_ReturnsOk() {
            // Arrange
            var token = "test-token";
            _tokenReaderMock.Setup(tr => tr.GetToken()).Returns(token);

            // Act
            var result = await _controller.SignOut(_tokenReaderMock.Object);

            // Assert
            var okResult = result as OkResult;
            okResult.Should().NotBeNull();
            _authServiceMock.Verify(s => s.SignOut(token), Times.Once);
        }

        [Fact]
        public async Task SignOut_MissingToken_ReturnsUnauthorized() {
            // Arrange
            _tokenReaderMock.Setup(tr => tr.GetToken()).Returns((string)null);

            // Act
            var result = await _controller.SignOut(_tokenReaderMock.Object);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.Value.Should().Be("Authorization token is missing or invalid.");
        }

        [Fact]
        public async Task RefreshToken_ValidRequest_ReturnsOk() {
            // Arrange
            var token = "test-token";
            var refreshTokenRequest = new RefreshTokenRequest { Email = "test@example.com", RefreshToken = "refresh-token" };
            var refreshment = new TokenRefreshment { Email = "test@example.com", JWT = token, RefreshToken = "refresh-token" };
            var jwt = "new-jwt";
            var newRefreshToken = "new-refresh-token";
            var resultToken = new Token { JWT = jwt, RefreshToken = newRefreshToken };

            _tokenReaderMock.Setup(tr => tr.GetToken()).Returns(token);
            _authServiceMock.Setup(s => s.RefreshToken(refreshment)).ReturnsAsync(resultToken);

            // Act
            var result = await _controller.RefreshToken(_tokenReaderMock.Object, refreshTokenRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(new { success = true, token = jwt, refreshToken = newRefreshToken });
        }

        [Fact]
        public async Task RefreshToken_MissingToken_ReturnsUnauthorized() {
            // Arrange
            var refreshTokenRequest = new RefreshTokenRequest { Email = "test@example.com", RefreshToken = "refresh-token" };
            _tokenReaderMock.Setup(tr => tr.GetToken()).Returns((string)null);

            // Act
            var result = await _controller.RefreshToken(_tokenReaderMock.Object, refreshTokenRequest);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.Value.Should().Be("Authorization token is missing or invalid.");
        }

        [Fact]
        public async Task RefreshToken_InvalidRequest_ReturnsBadRequest() {
            // Arrange
            var token = "test-token";
            var refreshTokenRequest = new RefreshTokenRequest { Email = "test@example.com", RefreshToken = "refresh-token" };
            var refreshment = new TokenRefreshment { Email = "test@example.com", JWT = token, RefreshToken = "refresh-token" };
            var validationFailures = new List<ValidationFailure> { new ValidationFailure("Email", "Invalid email format") };

            _tokenReaderMock.Setup(tr => tr.GetToken()).Returns(token);
            _authServiceMock.Setup(s => s.RefreshToken(refreshment)).ThrowsAsync(new ValidationException(validationFailures));

            // Act
            var result = await _controller.RefreshToken(_tokenReaderMock.Object, refreshTokenRequest);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.Value.Should().Be("Invalid email format");
        }
    }
}
