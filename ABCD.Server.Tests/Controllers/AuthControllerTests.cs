using ABCD.Application;
using ABCD.Application.Exceptions;
using ABCD.Lib;
using ABCD.Server.Controllers;
using ABCD.Server.Models;
using ABCD.Server.Requests;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace ABCD.Server.Tests.Controllers {
    public class AuthControllerTests {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ITypeMapper> _mapperMock;
        private readonly Mock<BearerTokenReader> _tokenReaderMock;
        private readonly AuthController _controller;

        public AuthControllerTests() {
            _authServiceMock = new Mock<IAuthService>();
            _mapperMock = new Mock<ITypeMapper>();
            _tokenReaderMock = new Mock<BearerTokenReader>(Mock.Of<IHttpContextAccessor>());
            _controller = new AuthController(_authServiceMock.Object, _mapperMock.Object);

            _controller.ControllerContext = new ControllerContext {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task SignIn_MissingRequiredParameters_ReturnsBadRequest() {
            // Arrange
            var request = new SignInRequest { Email = null, Password = null };
            var credentials = new SignInCommand { Email = null, Password = null };
            string message = "Missing parameters";
            _mapperMock.Setup(m => m.Map<SignInRequest, SignInCommand>(request)).Returns(credentials);
            _authServiceMock.Setup(s => s.SignIn(credentials)).ThrowsAsync(
                new ValidationException(new List<ValidationFailure> { new ValidationFailure { ErrorMessage = message } }));

            // Act
            var result = await _controller.SignIn(request);

            // Assert
            _authServiceMock.Verify(s => s.SignIn(credentials), Times.Once);
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal(message, badRequestResult.Value);
        }

        [Fact]
        public async Task SignIn_InvalidCredentials_ReturnsUnauthorized() {
            // Arrange
            var request = new SignInRequest { Email = "test@example.com", Password = "password" };
            var credentials = new SignInCommand { Email = "test@example.com", Password = "password" };
            string message = "Invalid login attempt.";
            _mapperMock.Setup(m => m.Map<SignInRequest, SignInCommand>(request)).Returns(credentials);
            _authServiceMock.Setup(s => s.SignIn(credentials)).ThrowsAsync(new SignInFailedException(message));

            // Act
            var result = await _controller.SignIn(request);

            // Assert
            _authServiceMock.Verify(s => s.SignIn(credentials), Times.Once);
            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(message, unauthorizedResult.Value);
        }

        [Fact]
        public async Task SignIn_ValidCredentials_ReturnsOk() {
            // Arrange
            var signInRequest = new SignInRequest { Email = "test@example.com", Password = "password" };
            var credentials = new SignInCommand { Email = "test@example.com", Password = "password" };
            var jwt = "test-token";
            var refreshToken = "test-refresh-token";
            var token = new Token { JWT = jwt, RefreshToken = refreshToken };

            _mapperMock.Setup(m => m.Map<SignInRequest, SignInCommand>(signInRequest)).Returns(credentials);
            _authServiceMock.Setup(s => s.SignIn(credentials)).ReturnsAsync(token);

            // Act
            var result = await _controller.SignIn(signInRequest);

            // Assert
            _authServiceMock.Verify(s => s.SignIn(credentials), Times.Once);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equivalent(new { success = true }, okResult.Value);
        }

        [Fact]
        public async Task SignOut_ValidToken_ReturnsOk() {
            // Arrange
            var token = "test-token";
            _tokenReaderMock.Setup(tr => tr.GetAccessToken()).Returns(token);

            // Act
            var result = await _controller.SignOut(_tokenReaderMock.Object);

            // Assert
            _authServiceMock.Verify(s => s.SignOut(token), Times.Once);
            var okResult = result as OkResult;
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task SignOut_MissingToken_ReturnsUnauthorized() {
            // Arrange
            _tokenReaderMock.Setup(tr => tr.GetAccessToken()).Returns((string)null);

            // Act
            var result = await _controller.SignOut(_tokenReaderMock.Object);

            // Assert
            _authServiceMock.Verify(s => s.SignOut(It.IsAny<string>()), Times.Never);
            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal("Authorization token is missing or invalid.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ValidRequest_ReturnsOk() {
            // Arrange
            var accessToken = "test-access-token";
            var refreshToken = "test-refresh-token";
            var refreshTokenRequest = new RefreshTokenRequest { Email = "test@example.com" };
            var refreshCommand = new RefreshTokenCommand { Email = "test@example.com", JWT = accessToken, RefreshToken = refreshToken };
            var jwt = "new-jwt";
            var newRefreshToken = "new-refresh-token";
            var resultToken = new Token { JWT = jwt, RefreshToken = newRefreshToken };

            _tokenReaderMock.Setup(tr => tr.GetAccessToken()).Returns(accessToken);
            _tokenReaderMock.Setup(tr => tr.GetRefreshToken()).Returns(refreshToken);
            _authServiceMock.Setup(s => s.RefreshToken(refreshCommand)).ReturnsAsync(resultToken);

            // Act
            var result = await _controller.RefreshToken(_tokenReaderMock.Object, refreshTokenRequest);

            // Assert
            _authServiceMock.Verify(s => s.RefreshToken(refreshCommand), Times.Once);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equivalent(new { success = true }, okResult.Value);
            
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RefreshToken_EmptyNullWhitespaceAccessToken_ReturnsUnauthorized(string accessToken) {
            // Arrange
            var refreshToken = "test-refresh-token";
            var refreshTokenRequest = new RefreshTokenRequest { Email = "test@example.com" };
            _tokenReaderMock.Setup(tr => tr.GetAccessToken()).Returns(accessToken);
            _tokenReaderMock.Setup(tr => tr.GetRefreshToken()).Returns(refreshToken);

            // Act
            var result = await _controller.RefreshToken(_tokenReaderMock.Object, refreshTokenRequest);

            // Assert
            _authServiceMock.Verify(s => s.RefreshToken(It.IsAny<RefreshTokenCommand>()), Times.Never);
            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal("Authorization token is missing or invalid.", unauthorizedResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RefreshToken_EmptyNullWhitespaceRefreshToken_ReturnsUnauthorized(string refreshToken) {
            // Arrange
            var accessToken = "test-access-token";
            var refreshTokenRequest = new RefreshTokenRequest { Email = "test@example.com" };
            _tokenReaderMock.Setup(tr => tr.GetAccessToken()).Returns(accessToken);
            _tokenReaderMock.Setup(tr => tr.GetRefreshToken()).Returns(refreshToken);

            // Act
            var result = await _controller.RefreshToken(_tokenReaderMock.Object, refreshTokenRequest);

            // Assert
            _authServiceMock.Verify(s => s.RefreshToken(It.IsAny<RefreshTokenCommand>()), Times.Never);
            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal("Refresh token is missing or invalid.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task RefreshToken_InvalidRequest_ReturnsBadRequest() {
            // Arrange
            var accessToken = "access-token";
            var refreshToken = "refresh-token";
            var refreshTokenRequest = new RefreshTokenRequest { Email = "test@example.com" };
            var refreshCommand = new RefreshTokenCommand { Email = "test@example.com", JWT = accessToken, RefreshToken = "refresh-token" };
            var validationFailures = new List<ValidationFailure> { new ValidationFailure("Email", "Invalid email format") };

            _tokenReaderMock.Setup(tr => tr.GetAccessToken()).Returns(accessToken);
            _tokenReaderMock.Setup(tr => tr.GetRefreshToken()).Returns(refreshToken);
            _authServiceMock.Setup(s => s.RefreshToken(refreshCommand)).ThrowsAsync(new ValidationException(validationFailures));

            // Act
            var result = await _controller.RefreshToken(_tokenReaderMock.Object, refreshTokenRequest);

            // Assert
            _authServiceMock.Verify(s => s.RefreshToken(refreshCommand), Times.Once);
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal("Invalid email format", badRequestResult.Value);
        }
    }
}
