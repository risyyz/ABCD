namespace ABCD.Lib {
    public class JwtSettings {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}
