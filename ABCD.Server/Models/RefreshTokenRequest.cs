namespace ABCD.Server.Models {
    public record RefreshTokenRequest {
        public required string Email { get; init; }
    }
}
