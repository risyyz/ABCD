using ABCD.Services.Crypto;

namespace ABCD.Services.Tests.Crypto {
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
            Assert.False(string.IsNullOrEmpty(encrypted));
            Assert.NotEqual(input, encrypted);
        }

        [Fact]
        public void Decrypt_ShouldReturnOriginalString() {
            // Arrange
            string input = "Hello, World!";
            string encrypted = cryptoService.Encrypt(input);

            // Act
            string decrypted = cryptoService.Decrypt(encrypted);

            // Assert
            Assert.Equal(input, decrypted);
        }

        [Fact]
        public void Encrypt_ShouldThrowArgumentNullException_WhenInputIsNull() {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => cryptoService.Encrypt(null));
        }

        [Fact]
        public void Decrypt_ShouldThrowArgumentNullException_WhenInputIsNull() {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => cryptoService.Decrypt(null));
        }

        [Fact]
        public void Decrypt_ShouldThrowFormatException_WhenInputIsNotBase64() {
            // Arrange
            string invalidBase64 = "InvalidBase64String";

            // Act & Assert
            Assert.Throws<FormatException>(() => cryptoService.Decrypt(invalidBase64));
        }
    }
}