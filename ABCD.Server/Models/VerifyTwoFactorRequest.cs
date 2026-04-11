namespace ABCD.Server.Models {
    public record VerifyTwoFactorRequest {
        public required string Email { get; init; }
        public required string Pin { get; init; }
    }
}
