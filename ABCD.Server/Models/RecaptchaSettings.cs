namespace ABCD.Server.Models {
    public class RecaptchaSettings {
        public string ProjectId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public float ScoreThreshold { get; set; } = 0.5f;
    }
}
