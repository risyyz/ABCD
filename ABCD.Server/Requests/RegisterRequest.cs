namespace ABCD.Server.Requests {
    public record RegisterRequest {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string PasswordConfirmation { get; init; }
    }
}
