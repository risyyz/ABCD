using Microsoft.Extensions.Caching.Memory;

namespace ABCD.Server.Middlewares {

    /// <summary>
    /// if a token is present in the invalidated token cache, the request is rejected
    /// </summary>
    public class TokenValidationMiddleware {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public TokenValidationMiddleware(RequestDelegate next, IMemoryCache cache) {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context) {
            if (context.Request.Headers.ContainsKey("Authorization")) {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (_cache.TryGetValue(token, out _)) {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("invalidated token");
                    return;
                }
            }

            await _next(context);
        }
    }
}
