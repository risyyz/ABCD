using System.Security.Cryptography;
using System.Text;

namespace ABCD.Services.Crypto {
    public class CryptoService : ICryptoService {

        private const int KeySizeBits = 256; // Key size in bits
        private const int KeySizeBytes = KeySizeBits / 8; // Key size in bytes
        private const int IvSizeBytes = 16; // IV size in bytes (128 bits)

        private readonly byte[] key;

        public CryptoService(string passphrase) {
            if (string.IsNullOrEmpty(passphrase)) {
                throw new ArgumentNullException(nameof(passphrase));
            }

            using (var sha256 = SHA256.Create()) {
                byte[] keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
                key = new byte[KeySizeBytes]; // 256 bits / 8 = 32 bytes                
                Array.Copy(keyHash, 0, key, 0, KeySizeBytes);
            }
        }

        public string Encrypt(string plainText) {
            if (string.IsNullOrEmpty(plainText)) {
                throw new ArgumentNullException(nameof(plainText));
            }

            using (var aes = Aes.Create()) {
                aes.GenerateIV();
                var iv = aes.IV;
                var encryptor = aes.CreateEncryptor(key, iv);

                using (var ms = new MemoryStream()) {
                    // Prepend the IV to the encrypted data
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                        using (var sw = new StreamWriter(cs)) {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string encrypted) {
            if (string.IsNullOrEmpty(encrypted)) {
                throw new ArgumentNullException(nameof(encrypted));
            }

            var buffer = Convert.FromBase64String(encrypted);

            using (var aes = Aes.Create()) {
                var iv = ExtractIv(buffer);
                var decryptor = aes.CreateDecryptor(key, iv);

                using (var ms = new MemoryStream(buffer, IvSizeBytes, buffer.Length - IvSizeBytes)) {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                        using (var sr = new StreamReader(cs)) {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        private byte[] ExtractIv(byte[] buffer) {
            var iv = new byte[IvSizeBytes];
            Array.Copy(buffer, 0, iv, 0, IvSizeBytes);
            return iv;
        }
    }
}
