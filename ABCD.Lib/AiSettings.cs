namespace ABCD.Lib {
    public class AiSettings {
        public const string SectionName = "Ai";
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";
    }
}
