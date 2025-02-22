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
        public async Task Login_ValidCredentials_ReturnsOk() {
            // Arrange
            var loginRequest = new LoginRequestModel { Email = "test@example.com", Password = "password" };
            var userLogin = new UserLogin { Email = "test@example.com", Password = "password" };
            var token = "test-token";
            var refreshToken = "test-refresh-token";

            _mapperMock.Setup(m => m.Map<UserLogin>(loginRequest)).Returns(userLogin);
            _authServiceMock.Setup(s => s.LoginUser(userLogin)).ReturnsAsync((token, refreshToken));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(new { Token = token, RefreshToken = refreshToken });
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized() {
            // Arrange
            var loginRequest = new LoginRequestModel { Email = "test@example.com", Password = "password" };
            var userLogin = new UserLogin { Email = "test@example.com", Password = "password" };
            string message = "Invalid login attempt.";
            _mapperMock.Setup(m => m.Map<UserLogin>(loginRequest)).Returns(userLogin);
            _authServiceMock.Setup(s => s.LoginUser(userLogin)).ThrowsAsync(new LoginFailedException(message));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.Value.Should().Be(message);
        }

        [Fact]
        public async Task Logout_ValidToken_ReturnsOk() {
            // Arrange
            var token = "test-token";
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext {
                HttpContext = context
            };

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = result as OkResult;
            okResult.Should().NotBeNull();
            _authServiceMock.Verify(s => s.InvalidateToken(token), Times.Once);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOk() {
            // Arrange
            var registerRequest = new RegisterRequestModel { Email = "test@example.com", Password = "password", PasswordConfirmation = "password" };
            var userRegistration = new UserRegistration { Email = "test@example.com", Password = "password", PasswordConfirmation = "password" };

            _mapperMock.Setup(m => m.Map<UserRegistration>(registerRequest)).Returns(userRegistration);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().Be("User registered successfully.");
            _authServiceMock.Verify(s => s.RegisterUser(userRegistration), Times.Once);
        }

        [Fact]
        public async Task Register_InvalidRequest_ReturnsBadRequest() {
            // Arrange
            var registerRequest = new RegisterRequestModel { Email = "test@example.com", Password = "password", PasswordConfirmation = "password" };
            var userRegistration = new UserRegistration { Email = "test@example.com", Password = "password", PasswordConfirmation = "password" };
            var message = "Invalid data";
            _mapperMock.Setup(m => m.Map<UserRegistration>(registerRequest)).Returns(userRegistration);
            _authServiceMock.Setup(s => s.RegisterUser(userRegistration)).ThrowsAsync(new ValidationException(message, new List<ValidationFailure> { new ValidationFailure { ErrorMessage = message } }));

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.Value.Should().Be(message);
        }
    }
}
