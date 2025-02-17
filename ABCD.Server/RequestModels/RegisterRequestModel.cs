namespace ABCD.Server.RequestModels {
    public record RegisterRequestModel {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string PasswordConfirmation { get; init; }
    }
}
