namespace ABCD.Server.Requests {
    public record RegisterUserRequest {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string PasswordConfirmation { get; init; }
    }
}
