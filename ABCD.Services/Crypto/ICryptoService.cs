namespace ABCD.Services.Crypto {
    public interface ICryptoService {
        string Encrypt(string plainText);
        string Decrypt(string encrypted);
    }
}
