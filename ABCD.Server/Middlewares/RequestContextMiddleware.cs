using ABCD.Domain;
using ABCD.Lib;
using ABCD.Lib.Exceptions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ABCD.Server.Middlewares {
    public class RequestContextMiddleware {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly CachingSettings _cachingSettings;

        public RequestContextMiddleware(RequestDelegate next, IMemoryCache cache, IOptions<CachingSettings> cachingSettings) {
            _next = next;
            _cache = cache;
            _cachingSettings = cachingSettings.Value;
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestContextAccessor contextAccessor, IBlogRepository blogRepository) {
            var domain = httpContext.Request.Host.Host.ToLowerInvariant();
            if (!_cache.TryGetValue(domain, out Blog? blog)) 
                blog = await blogRepository.GetByDomainAsync(domain);            

            if (blog == null)
                throw new RequestContextException($"Unable to resolve blog from domain '{domain}'");

            _cache.Set(domain, blog, TimeSpan.FromMinutes(_cachingSettings.DomainCacheDurationInMinutes));
            contextAccessor.RequestContext = new RequestContext(blog);
            await _next(httpContext);
        }
    }
}
