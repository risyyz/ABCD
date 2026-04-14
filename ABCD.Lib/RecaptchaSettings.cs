namespace ABCD.Lib {
    public class RecaptchaSettings {
        public string ApiKey { get; set; } = string.Empty;
        public float ScoreThreshold { get; set; } = 0.5f;
    }
}
