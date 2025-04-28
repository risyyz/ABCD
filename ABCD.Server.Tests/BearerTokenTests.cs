using Microsoft.AspNetCore.Http;

using Moq;

namespace ABCD.Server.Tests {
    public class BearerTokenReaderTests {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly BearerTokenReader _tokenReader;

        public BearerTokenReaderTests() {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _tokenReader = new BearerTokenReader(_httpContextAccessorMock.Object);
        }

        [Fact]
        public void GetToken_ValidAuthorizationHeader_ReturnsToken() {
            // Arrange
            var token = "test-token";
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            // Act
            var result = _tokenReader.GetToken();

            // Assert
            Assert.Equal(token, result);
        }

        [Fact]
        public void GetToken_MissingAuthorizationHeader_ReturnsNull() {
            // Arrange
            var context = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            // Act
            var result = _tokenReader.GetToken();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetToken_EmptyAuthorizationHeader_ReturnsNull() {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = string.Empty;
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            // Act
            var result = _tokenReader.GetToken();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetToken_InvalidAuthorizationHeader_ReturnsNull() {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "InvalidHeader";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            // Act
            var result = _tokenReader.GetToken();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetToken_AuthorizationHeaderWithoutBearer_ReturnsNull() {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Basic test-token";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            // Act
            var result = _tokenReader.GetToken();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetToken_NullHttpContext_ReturnsNull() {
            // Arrange
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

            // Act
            var result = _tokenReader.GetToken();

            // Assert
            Assert.Null(result);
        }
    }
}

