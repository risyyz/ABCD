namespace ABCD.Lib {
    public class EmailSettings {
        public const string SectionName = "Email";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        // Add for drop folder support
        public string? PickupDirectory { get; set; }
    }
}
