namespace ABCD.Lib {
    public class AuthCookieSettings {
        public const string SectionName = "AuthCookie";
        public int AccessTokenExpiryInMinutes { get; set; } = 60;
        public int RefreshTokenExpiryInMinutes { get; set; } = 60;
    }
}
