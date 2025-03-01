namespace ABCD.Server.RequestModels {
    public record SignInRequestModel {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}
