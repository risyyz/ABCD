namespace ABCD.Server.Models {
    public record ChangePasswordRequest {
        public required string Pin { get; init; }
        public required string NewPassword { get; init; }
    }
}
