namespace ABCD.Server.Requests {
    public record SignInRequest {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
