
namespace ABCD.Application.Tests {
    using global::ABCD.Lib;

    using Xunit;

    public class RegisterUserCommandValidatorTests {
        private PasswordPolicy _passwordPolicy;

        private RegisterUserCommandValidator CreateValidator() {
            return new RegisterUserCommandValidator(_passwordPolicy);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Validate_EmailIsEmptyOrWhitespace_ShouldHaveValidationError(string email) {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15 };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = email,
                Password = "ValidPassword123!",
                PasswordConfirmation = "ValidPassword123!"
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void Validate_EmailIsInvalid_ShouldHaveValidationError() {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15 };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "invalid-email",
                Password = "ValidPassword123!",
                PasswordConfirmation = "ValidPassword123!"
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Invalid email format.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Validate_PasswordIsEmptyOrWhitespace_ShouldHaveValidationError(string password) {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15 };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = password,
                PasswordConfirmation = password
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == "Password is required.");
        }

        [Fact]
        public void Validate_PasswordIsTooShort_ShouldHaveValidationError() {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15 };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = "Short1!",
                PasswordConfirmation = "Short1!"
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == $"Password must be at least {_passwordPolicy.MinPasswordLength} characters long.");
        }

        [Theory]
        [InlineData(true, "VALIDPASSWORD123!", "Password must contain at least one lowercase letter.")]
        [InlineData(false, "VALIDPASSWORD123!", null)]
        public void Validate_PasswordMissingLowercase_ShouldHandlePolicy(bool requireLowercase, string password, string expectedErrorMessage) {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15, RequireLowercase = requireLowercase };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = password,
                PasswordConfirmation = password
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            if (requireLowercase) {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == expectedErrorMessage);
            } else {
                Assert.True(result.IsValid);
            }
        }

        [Theory]
        [InlineData(true, "validpassword123!", "Password must contain at least one uppercase letter.")]
        [InlineData(false, "validpassword123!", null)]
        public void Validate_PasswordMissingUppercase_ShouldHandlePolicy(bool requireUppercase, string password, string expectedErrorMessage) {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15, RequireUppercase = requireUppercase };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = password,
                PasswordConfirmation = password
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            if (requireUppercase) {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == expectedErrorMessage);
            } else {
                Assert.True(result.IsValid);
            }
        }

        [Theory]
        [InlineData(true, "ValidPassworddd!", "Password must contain at least one digit.")]
        [InlineData(false, "ValidPassworddd!", null)]
        public void Validate_PasswordMissingDigit_ShouldHandlePolicy(bool requireDigit, string password, string expectedErrorMessage) {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15, RequireDigit = requireDigit };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = password,
                PasswordConfirmation = password
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            if (requireDigit) {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == expectedErrorMessage);
            } else {
                Assert.True(result.IsValid);
            }
        }

        [Theory]
        [InlineData(true, "ValidPassword123", "Password must contain at least one special character.")]
        [InlineData(false, "ValidPassword123", null)]
        public void Validate_PasswordMissingSpecialCharacter_ShouldHandlePolicy(bool requireSpecialCharacter, string password, string expectedErrorMessage) {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15, RequireSpecialCharacter = requireSpecialCharacter };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = password,
                PasswordConfirmation = password
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            if (requireSpecialCharacter) {
                Assert.False(result.IsValid);
                Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == expectedErrorMessage);
            } else {
                Assert.True(result.IsValid);
            }
        }

        [Fact]
        public void Validate_PasswordsDoNotMatch_ShouldHaveValidationError() {
            // Arrange
            _passwordPolicy = new PasswordPolicy { MinPasswordLength = 15 };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = "ValidPassword123!",
                PasswordConfirmation = "DifferentPassword123!"
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PasswordConfirmation" && e.ErrorMessage == "Passwords do not match.");
        }

        [Fact]
        public void Validate_ValidRegistration_ShouldNotHaveValidationErrors() {
            // Arrange
            _passwordPolicy = new PasswordPolicy {
                MinPasswordLength = 15,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireDigit = true,
                RequireSpecialCharacter = true
            };
            var validator = CreateValidator();
            var registration = new RegisterUserCommand {
                Email = "user@example.com",
                Password = "ValidPassword123!",
                PasswordConfirmation = "ValidPassword123!"
            };

            // Act
            var result = validator.Validate(registration);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
