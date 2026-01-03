namespace ABCD.Application.Tests {
    public class TokenRefreshmentValidatorTests {
        private readonly TokenRefreshmentValidator _validator;

        public TokenRefreshmentValidatorTests() {
            _validator = new TokenRefreshmentValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Validate_EmailIsEmptyOrWhitespace_ShouldHaveValidationError(string email) {
            // Arrange
            var tokenRefreshment = new RefreshTokenCommand {
                Email = email,
                JWT = "valid-jwt",
                RefreshToken = "valid-refresh-token"
            };

            // Act
            var result = _validator.Validate(tokenRefreshment);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required");
        }

        [Fact]
        public void Validate_EmailIsInvalid_ShouldHaveValidationError() {
            // Arrange
            var tokenRefreshment = new RefreshTokenCommand {
                Email = "invalid-email",
                JWT = "valid-jwt",
                RefreshToken = "valid-refresh-token"
            };

            // Act
            var result = _validator.Validate(tokenRefreshment);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Validate_JWTIsEmptyOrWhitespace_ShouldHaveValidationError(string jwt) {
            // Arrange
            var tokenRefreshment = new RefreshTokenCommand {
                Email = "user@example.com",
                JWT = jwt,
                RefreshToken = "valid-refresh-token"
            };

            // Act
            var result = _validator.Validate(tokenRefreshment);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "JWT" && e.ErrorMessage == "JWT is required");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Validate_RefreshTokenIsEmptyOrWhitespace_ShouldHaveValidationError(string refreshToken) {
            // Arrange
            var tokenRefreshment = new RefreshTokenCommand {
                Email = "user@example.com",
                JWT = "valid-jwt",
                RefreshToken = refreshToken
            };

            // Act
            var result = _validator.Validate(tokenRefreshment);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken" && e.ErrorMessage == "RefreshToken is required");
        }

        [Fact]
        public void Validate_ValidTokenRefreshment_ShouldNotHaveValidationErrors() {
            // Arrange
            var tokenRefreshment = new RefreshTokenCommand {
                Email = "user@example.com",
                JWT = "valid-jwt",
                RefreshToken = "valid-refresh-token"
            };

            // Act
            var result = _validator.Validate(tokenRefreshment);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
