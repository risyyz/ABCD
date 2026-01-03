using ABCD.Server.Middlewares;

using FluentAssertions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Memory;

using Moq;

namespace ABCD.Server.Tests.Middlewares {
    public class TokenValidationMiddlewareTests {
        private readonly DefaultHttpContext _context;
        private readonly Mock<HttpResponse> _responseMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<RequestDelegate> _nextMock;

        public TokenValidationMiddlewareTests() {
            _context = new DefaultHttpContext();
            _responseMock = new Mock<HttpResponse>();
            _context.Response.Body = new MemoryStream();
            _context.Response.StatusCode = StatusCodes.Status200OK;

            _cacheMock = new Mock<IMemoryCache>();
            _nextMock = new Mock<RequestDelegate>();
        }

        [Fact]
        public async Task InvokeAsync_TokenInCache_ReturnsUnauthorized() {
            // Arrange
            var token = "test-token";
            _context.Request.Headers["Cookie"] = $"access_token={token}";

            object cachedValue = true;
            _cacheMock.Setup(m => m.TryGetValue(token, out cachedValue)).Returns(true);

            // Mock GetEndpoint to return an endpoint with AuthorizeAttribute
            var endpoint = new Endpoint(
                (HttpContext ctx) => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()),
                "Test endpoint"
            );
            var feature = new Mock<IEndpointFeature>();
            feature.Setup(f => f.Endpoint).Returns(endpoint);
            _context.Features.Set<IEndpointFeature>(feature.Object);

            var middleware = new TokenValidationMiddleware(_nextMock.Object, _cacheMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(_context.Response.Body);
            var responseText = await reader.ReadToEndAsync();
            responseText.Should().Be("invalid token");
            _nextMock.Verify(m => m(It.IsAny<HttpContext>()), Times.Never);
            _cacheMock.Verify(m => m.TryGetValue(token, out cachedValue), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_TokenNotInCache_CallsNextMiddleware() {
            // Arrange
            var token = "test-token";
            _context.Request.Headers["Cookie"] = $"access_token={token}";

            object cachedValue;
            _cacheMock.Setup(m => m.TryGetValue(token, out cachedValue)).Returns(false);
            _nextMock.Setup(m => m(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Mock GetEndpoint to return an endpoint with AuthorizeAttribute
            var endpoint = new Endpoint(
                (HttpContext ctx) => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()),
                "Test endpoint"
            );
            var feature = new Mock<IEndpointFeature>();
            feature.Setup(f => f.Endpoint).Returns(endpoint);
            _context.Features.Set<IEndpointFeature>(feature.Object);

            var middleware = new TokenValidationMiddleware(_nextMock.Object, _cacheMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().NotBe(StatusCodes.Status401Unauthorized);
            _nextMock.Verify(m => m(It.IsAny<HttpContext>()), Times.Once);
            _cacheMock.Verify(m => m.TryGetValue(token, out cachedValue), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_NoAccessTokenCookie_CallsNextMiddleware() {
            // Arrange
            // No access_token cookie set
            // Mock GetEndpoint to return an endpoint with AuthorizeAttribute
            var endpoint = new Endpoint(
                (HttpContext ctx) => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()),
                "Test endpoint"
            );
            var feature = new Mock<IEndpointFeature>();
            feature.Setup(f => f.Endpoint).Returns(endpoint);
            _context.Features.Set<IEndpointFeature>(feature.Object);

            var middleware = new TokenValidationMiddleware(_nextMock.Object, _cacheMock.Object);
            _nextMock.Setup(m => m(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().NotBe(StatusCodes.Status401Unauthorized);
            _nextMock.Verify(m => m(It.IsAny<HttpContext>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task InvokeAsync_TokenEmptyWhitespace_CallsNextMiddleware(string accessToken) {
            // Arrange
            _context.Request.Headers["Cookie"] = $"access_token={accessToken}";

            object cachedValue = true;
            _cacheMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out cachedValue)).Returns(true);

            // Mock GetEndpoint to return an endpoint with AuthorizeAttribute
            var endpoint = new Endpoint(
                (HttpContext ctx) => Task.CompletedTask,
                new EndpointMetadataCollection(new AuthorizeAttribute()),
                "Test endpoint"
            );
            var feature = new Mock<IEndpointFeature>();
            feature.Setup(f => f.Endpoint).Returns(endpoint);
            _context.Features.Set<IEndpointFeature>(feature.Object);

            var middleware = new TokenValidationMiddleware(_nextMock.Object, _cacheMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().NotBe(StatusCodes.Status401Unauthorized);
            _nextMock.Verify(m => m(It.IsAny<HttpContext>()), Times.Once);
            _cacheMock.Verify(m => m.TryGetValue(It.IsAny<string>(), out cachedValue), Times.Never);
        }
    }
}
