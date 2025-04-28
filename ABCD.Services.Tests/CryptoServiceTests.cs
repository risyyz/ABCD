using FluentAssertions;

namespace ABCD.Services.Tests {
    public class CryptoServiceTests {
        private readonly string passphrase = "3q2+7w==...";
        private readonly ICryptoService cryptoService;

        public CryptoServiceTests() {
            cryptoService = new CryptoService(passphrase);
        }

        [Fact]
        public void Encrypt_ShouldReturnEncryptedString() {
            // Arrange
            string input = "Hello, World!";

            // Act
            string encrypted = cryptoService.Encrypt(input);

            // Assert
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBe(input);
        }

        [Fact]
        public void Decrypt_ShouldReturnOriginalString() {
            // Arrange
            string input = "Hello, World!";
            string encrypted = cryptoService.Encrypt(input);

            // Act
            string decrypted = cryptoService.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(input);
        }

        [Fact]
        public void Encrypt_ShouldThrowArgumentNullException_WhenInputIsNull() {
            // Act
            Action act = () => cryptoService.Encrypt(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Decrypt_ShouldThrowArgumentNullException_WhenInputIsNull() {
            // Act
            Action act = () => cryptoService.Decrypt(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Decrypt_ShouldThrowFormatException_WhenInputIsNotBase64() {
            // Arrange
            string invalidBase64 = "InvalidBase64String";

            // Act
            Action act = () => cryptoService.Decrypt(invalidBase64);

            // Assert
            act.Should().Throw<FormatException>();
        }
    }
}
