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
                
        [Command("key", Description = "dotnet run -- key --length 64")]
        public void GenerateKey(int length) {
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
    }
}

