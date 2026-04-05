namespace ABCD.Lib {
    public record TwoFactorChallenge {
        public required string Email { get; init; }
        public bool RequiresTwoFactor => true;
    }
}
