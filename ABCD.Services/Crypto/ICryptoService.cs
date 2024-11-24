namespace ABCD.Services.Crypto {
    public interface ICryptoService {
        string Encrypt(string input);
        string Decrypt(string input);
    }
}
