namespace ABCD.Lib {
    public class PasswordPolicy {
        public int MinPasswordLength { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireSpecialCharacter { get; set; }
    }
}
