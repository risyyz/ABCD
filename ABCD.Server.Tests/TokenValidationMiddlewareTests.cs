using ABCD.Server.Middlewares;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

using Moq;

namespace ABCD.Server.Tests {
    public class TokenValidationMiddlewareTests {
        private readonly DefaultHttpContext _context;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<RequestDelegate> _nextMock;

        public TokenValidationMiddlewareTests() {
            _context = new DefaultHttpContext();
            _context.Response.Body = new MemoryStream();
            _cacheMock = new Mock<IMemoryCache>();
            _nextMock = new Mock<RequestDelegate>();
        }

        [Fact]
        public async Task InvokeAsync_TokenInCache_ReturnsUnauthorized() {
            // Arrange
            var token = "test-token";
            _context.Request.Headers["Authorization"] = $"Bearer {token}";

            object cachedValue = true;
            _cacheMock.Setup(m => m.TryGetValue(token, out cachedValue)).Returns(true);

            var middleware = new TokenValidationMiddleware(_nextMock.Object, _cacheMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(_context.Response.Body);
            var responseText = await reader.ReadToEndAsync();
            responseText.Should().Be("invalidated token");
            _nextMock.Verify(m => m(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_TokenNotInCache_CallsNextMiddleware() {
            // Arrange
            var token = "test-token";
            _context.Request.Headers["Authorization"] = $"Bearer {token}";

            object cachedValue;
            _cacheMock.Setup(m => m.TryGetValue(token, out cachedValue)).Returns(false);
            _nextMock.Setup(m => m(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            var middleware = new TokenValidationMiddleware(_nextMock.Object, _cacheMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().NotBe(StatusCodes.Status401Unauthorized);
            _nextMock.Verify(m => m(It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_NoAuthorizationHeader_CallsNextMiddleware() {
            // Arrange
            var middleware = new TokenValidationMiddleware(_nextMock.Object, _cacheMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().NotBe(StatusCodes.Status401Unauthorized);
            _nextMock.Verify(m => m(It.IsAny<HttpContext>()), Times.Once);
        }
    }
}
