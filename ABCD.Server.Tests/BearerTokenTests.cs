using ABCD.Application;

using Microsoft.AspNetCore.Http;

using Moq;

namespace ABCD.Server.Tests {
    public class BearerTokenReaderTests {
        [Fact]
        public void GetAccessToken_ReturnsToken_WhenCookieExists() {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = $"{AppConstants.ACCESS_TOKEN}=mytoken";
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetAccessToken();

            // Assert
            Assert.Equal("mytoken", token);
        }

        [Fact]
        public void GetAccessToken_ReturnsNull_WhenCookieMissing() {
            // Arrange
            var context = new DefaultHttpContext();
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetAccessToken();

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetAccessToken_ReturnsNull_WhenCookieIsWhitespace() {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = $"{AppConstants.ACCESS_TOKEN}=   ";
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetAccessToken();

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetAccessToken_ReturnsNull_WhenHttpContextIsNull() {
            // Arrange
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns((HttpContext)null);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetAccessToken();

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetRefreshToken_ReturnsToken_WhenCookieExists() {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = $"{AppConstants.REFRESH_TOKEN}=myrefreshtoken";
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetRefreshToken();

            // Assert
            Assert.Equal("myrefreshtoken", token);
        }

        [Fact]
        public void GetRefreshToken_ReturnsNull_WhenCookieMissing() {
            // Arrange
            var context = new DefaultHttpContext();
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetRefreshToken();

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetRefreshToken_ReturnsNull_WhenCookieIsWhitespace() {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Cookie"] = $"{AppConstants.REFRESH_TOKEN}=   ";
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetRefreshToken();

            // Assert
            Assert.Null(token);
        }

        [Fact]
        public void GetRefreshToken_ReturnsNull_WhenHttpContextIsNull() {
            // Arrange
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns((HttpContext)null);
            var reader = new BearerTokenReader(accessor.Object);

            // Act
            var token = reader.GetRefreshToken();

            // Assert
            Assert.Null(token);
        }
    }
}

