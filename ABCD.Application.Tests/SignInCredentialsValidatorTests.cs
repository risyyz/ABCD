namespace ABCD.Application.Tests {
    public class SignInCredentialsValidatorTests {
        private readonly SignInCredentialsValidator _validator;

        public SignInCredentialsValidatorTests() {
            _validator = new SignInCredentialsValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Validate_EmailIsEmpty_ShouldHaveValidationError(string email) {
            // Arrange
            var credentials = new SignInCredentials {
                Email = email,
                Password = "ValidPassword123"
            };

            // Act
            var result = _validator.Validate(credentials);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void Validate_EmailIsInvalid_ShouldHaveValidationError() {
            // Arrange
            var credentials = new SignInCredentials {
                Email = "invalid-email",
                Password = "ValidPassword123"
            };

            // Act
            var result = _validator.Validate(credentials);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Validate_PasswordIsEmpty_ShouldHaveValidationError(string password) {
            // Arrange
            var credentials = new SignInCredentials {
                Email = "user@example.com",
                Password = password
            };

            // Act
            var result = _validator.Validate(credentials);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == "Password is required.");
        }

        [Fact]
        public void Validate_ValidCredentials_ShouldNotHaveValidationErrors() {
            // Arrange
            var credentials = new SignInCredentials {
                Email = "user@example.com",
                Password = "ValidPassword123"
            };

            // Act
            var result = _validator.Validate(credentials);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
