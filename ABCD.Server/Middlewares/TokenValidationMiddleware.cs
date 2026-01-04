using ABCD.Application;

using Microsoft.AspNetCore.Authorization;
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
            var authorizeAttribute = context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>();
            if (authorizeAttribute != null && context.Request.Cookies.ContainsKey(AppConstants.ACCESS_TOKEN)) {
                var token = context.Request.Cookies[AppConstants.ACCESS_TOKEN];

                if (!string.IsNullOrWhiteSpace(token) && _cache.TryGetValue(token, out _)) {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("invalid token");
                    return;
                }
            }
            await _next(context);
        }
    }
}
