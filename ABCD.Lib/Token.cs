namespace ABCD.Lib {
    public record Token {
        public required string JWT { get; init; }
        public required string RefreshToken { get; init; }
        public bool IsEmpty { get => string.IsNullOrWhiteSpace(JWT) && string.IsNullOrWhiteSpace(RefreshToken); }
    }
}
