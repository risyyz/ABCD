namespace ABCD.Server.Requests {
    public record RefreshTokenRequest {
        public required string Email { get; init; }
        public required string RefreshToken { get; init; }
    }
}
