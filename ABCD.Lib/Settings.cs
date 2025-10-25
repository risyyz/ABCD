namespace ABCD.Lib {
    public class Settings {
        public string ConnectionString { get; set; } = string.Empty;
        public string CryptoPassPhrase { get; set; } = string.Empty;
    }

    public class CachingSettings {
        public const string SectionName = "Caching";
        public int DomainCacheDurationInMinutes { get; set; }
    }

}
