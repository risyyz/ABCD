using ABCD.Application;

namespace ABCD.Server {
    public class BearerTokenReader {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerTokenReader(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual string? GetAccessToken() {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies[AppConstants.ACCESS_TOKEN];
            return string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        }

        public virtual string? GetRefreshToken() {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies[AppConstants.REFRESH_TOKEN];
            return string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        }
    }
}
