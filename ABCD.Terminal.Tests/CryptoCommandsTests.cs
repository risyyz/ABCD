namespace ABCD.Terminal.Tests {
    public class CryptoCommandsTests {
        private readonly CryptoCommands cryptoCommands;

        public CryptoCommandsTests() {
            // Mock the ICryptoService if needed, but it's not used in GenerateKey
            cryptoCommands = new CryptoCommands(null);
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
            var generatedKey = output.ToString().Trim();
            Assert.StartsWith("Generated Key: ", generatedKey);
            var key = generatedKey.Substring("Generated Key: ".Length);
            Assert.Equal(length, key.Length);
        }

        [Fact]
        public void GenerateKey_ShouldThrowArgumentException_WhenLengthIsZero() {
            // Arrange
            int length = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => cryptoCommands.GenerateKey(length));
            Assert.Equal("Length must be a positive integer. (Parameter 'length')", exception.Message);
        }

        [Fact]
        public void GenerateKey_ShouldThrowArgumentException_WhenLengthIsNegative() {
            // Arrange
            int length = -1;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => cryptoCommands.GenerateKey(length));
            Assert.Equal("Length must be a positive integer. (Parameter 'length')", exception.Message);
        }
    }
}
