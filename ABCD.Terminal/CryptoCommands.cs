using System.Security.Cryptography;
using System.Text;

using ABCD.Services.Crypto;

using Cocona;

namespace ABCD.Terminal {
    public class CryptoCommands {

        private readonly ICryptoService cryptoService;

        public CryptoCommands(ICryptoService cryptoService) {
            this.cryptoService = cryptoService;
        }

        [Command("key", Description = "dotnet run -- key --l 64")]
        public void GenerateKey([Option("l", Description = "length of key string to be generated")] int length) {
            if (length <= 0) {
                throw new ArgumentException("Length must be a positive integer.", nameof(length));
            }

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=|:.<>?";
            var key = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create()) {
                var data = new byte[4];
                for (int i = 0; i < length; i++) {
                    rng.GetBytes(data);
                    var randomIndex = BitConverter.ToUInt32(data, 0) % chars.Length;
                    key.Append(chars[(int)randomIndex]);
                }
            }
            Console.WriteLine($"Generated Key: {key}");
        }

        [Command("encrypt", Description = "dotnet run -- encrypt --i \"Hello World\"")]
        public void Encrypt([Option("i", Description = "input string to be encrypted")] string input) {
            if (string.IsNullOrEmpty(input)) {
                throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            }

            string encrypted = cryptoService.Encrypt(input);
            Console.WriteLine($"Encrypted String: {encrypted}");
        }

        [Command("decrypt", Description = "dotnet run -- decrypt --i \"9LpRPlXIDTGOjUyl07WgqZPcu62EGuQSQSvM+CzlEt0=\"")]
        public void Decrypt([Option("i", Description = "input string to be decrypted")] string input) {
            if (string.IsNullOrEmpty(input)) {
                throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            }

            string decrypted = cryptoService.Decrypt(input);
            Console.WriteLine($"Decrypted String: {decrypted}");
        }
    }
}
