namespace ABCD.Server {
    public class BearerTokenReader {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerTokenReader(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual string? GetToken() {
            // Read token from HTTP-only cookie
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];

            if (string.IsNullOrWhiteSpace(token)) {
                return null; // Return null if the cookie is missing
            }

            return token.Trim(); // Return the token from cookie
        }
    }
}
