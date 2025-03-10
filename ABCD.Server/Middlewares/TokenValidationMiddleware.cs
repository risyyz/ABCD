using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace ABCD.Server.Middlewares {

    /// <summary>
    /// if a token is present in the invalidated token cache, the request is rejected
    /// </summary>
    public class TokenValidationMiddleware {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private const string AUTH_HEADER = "Authorization";

        public TokenValidationMiddleware(RequestDelegate next, IMemoryCache cache) {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context) {
            var authorizeAttribute = context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>();
            if (authorizeAttribute != null && context.Request.Headers.ContainsKey(AUTH_HEADER)) {
                var token = context.Request.Headers[AUTH_HEADER].ToString().Replace("Bearer ", "");

                if (_cache.TryGetValue(token, out _)) {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("invalid token");
                    return;
                }
            }
            await _next(context);
        }
    }
}
