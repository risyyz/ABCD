using ABCD.Services.Crypto;

using Moq;

namespace ABCD.Terminal.Tests {
    public class CryptoCommandsTests {
        private readonly Mock<ICryptoService> mockCryptoService;
        private readonly CryptoCommands cryptoCommands;

        public CryptoCommandsTests() {
            mockCryptoService = new Mock<ICryptoService>();
            cryptoCommands = new CryptoCommands(mockCryptoService.Object);
        }

        [Fact]
        public void GenerateKey_ShouldGenerateKeyOfSpecifiedLength() {
            // Arrange
            int length = 64;
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            cryptoCommands.GenerateKey(length);

            // Assert
            var result = output.ToString().Trim();
            Assert.StartsWith("Generated Key: ", result);
            string generatedKey = result.Substring("Generated Key: ".Length);
            Assert.Equal(length, generatedKey.Length);
        }

        [Fact]
        public void GenerateKey_ShouldThrowArgumentException_WhenLengthIsNotPositive() {
            // Arrange
            int length = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => cryptoCommands.GenerateKey(length));
            Assert.Equal("Length must be a positive integer. (Parameter 'length')", exception.Message);
        }

        [Fact]
        public void Encrypt_ShouldEncryptInputString() {
            // Arrange
            string input = "Hello World";
            string encryptedString = "EncryptedHelloWorld";
            mockCryptoService.Setup(cs => cs.Encrypt(input)).Returns(encryptedString);
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            cryptoCommands.Encrypt(input);

            // Assert
            var result = output.ToString().Trim();
            Assert.Equal($"Encrypted String: {encryptedString}", result);
        }

        [Fact]
        public void Encrypt_ShouldThrowArgumentException_WhenInputIsNullOrEmpty() {
            // Arrange
            string input = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => cryptoCommands.Encrypt(input));
            Assert.Equal("Input cannot be null or empty. (Parameter 'input')", exception.Message);
        }

        [Fact]
        public void Decrypt_ShouldDecryptInputString() {
            // Arrange
            string input = "EncryptedHelloWorld";
            string decryptedString = "Hello World";
            mockCryptoService.Setup(cs => cs.Decrypt(input)).Returns(decryptedString);
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            cryptoCommands.Decrypt(input);

            // Assert
            var result = output.ToString().Trim();
            Assert.Equal($"Decrypted String: {decryptedString}", result);
        }

        [Fact]
        public void Decrypt_ShouldThrowArgumentException_WhenInputIsNullOrEmpty() {
            // Arrange
            string input = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => cryptoCommands.Decrypt(input));
            Assert.Equal("Input cannot be null or empty. (Parameter 'input')", exception.Message);
        }
    }
}
