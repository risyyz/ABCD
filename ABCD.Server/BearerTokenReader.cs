namespace ABCD.Server {
    public class BearerTokenReader {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerTokenReader(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual string? GetToken() {
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
                return null; // Return null if the header is missing or invalid
            }

            return authorizationHeader["Bearer ".Length..].Trim(); // Extract and return the token
        }
    }
}
