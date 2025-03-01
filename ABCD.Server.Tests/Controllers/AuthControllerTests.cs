using ABCD.Lib;
using ABCD.Lib.Auth;
using ABCD.Lib.Exceptions;
using ABCD.Server.Controllers;
using ABCD.Server.RequestModels;
using ABCD.Services;

using AutoMapper;

using FluentAssertions;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace ABCD.Server.Tests.Controllers {
    public class AuthControllerTests {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthController _controller;

        public AuthControllerTests() {
            _authServiceMock = new Mock<IAuthService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new AuthController(_authServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task SignIn_MissingRequiredParameters_ReturnsBadRequest() {
            // Arrange
            var request = new SignInRequestModel { Email = null, Password = null };
            var credentials = new SignInCredentials { Email = null, Password = null };
            string message = "Missing parameters";
            _mapperMock.Setup(m => m.Map<SignInCredentials>(request)).Returns(credentials);
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
            var request = new SignInRequestModel { Email = "test@example.com", Password = "password" };
            var credentials = new SignInCredentials { Email = "test@example.com", Password = "password" };
            string message = "Invalid login attempt.";
            _mapperMock.Setup(m => m.Map<SignInCredentials>(request)).Returns(credentials);
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
            var signInRequest = new SignInRequestModel { Email = "test@example.com", Password = "password" };
            var credentials = new SignInCredentials { Email = "test@example.com", Password = "password" };
            var jwt = "test-token";
            var refreshToken = "test-refresh-token";
            var token = new Token { JWT = jwt, RefreshToken = refreshToken };

            _mapperMock.Setup(m => m.Map<SignInCredentials>(signInRequest)).Returns(credentials);
            _authServiceMock.Setup(s => s.SignIn(credentials)).ReturnsAsync(token);

            // Act
            var result = await _controller.SignIn(signInRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(new { token = jwt, refreshToken = refreshToken });
        }

        [Fact]
        public async Task SignOut_ValidToken_ReturnsOk() {
            // Arrange
            var token = "test-token";
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext {
                HttpContext = context
            };

            // Act
            var result = await _controller.SignOut();

            // Assert
            var okResult = result as OkResult;
            okResult.Should().NotBeNull();
            _authServiceMock.Verify(s => s.SignOut(token), Times.Once);
        }        
    }
}
