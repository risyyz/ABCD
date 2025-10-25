using ABCD.Core;
using ABCD.Data;
using ABCD.Lib;
using ABCD.Lib.Exceptions;

using Microsoft.EntityFrameworkCore;
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

        public async Task InvokeAsync(HttpContext httpContext, DataContext dataContext, RequestContextAccessor contextAccessor) {

            var domain = httpContext.Request.Host.Host.ToLowerInvariant();
            if (!_cache.TryGetValue(domain, out Blog? blog)) {
                blog = await (
                    from d in dataContext.BlogDomains
                    join b in dataContext.Blogs on d.BlogId equals b.BlogId
                    where d.DomainName.Value.ToLowerInvariant() == domain
                    select b
                ).FirstOrDefaultAsync();

                if (blog != null) {
                    _cache.Set(domain, blog, TimeSpan.FromMinutes(_cachingSettings.DomainCacheDurationInMinutes));
                }
            }

            if (blog == null)
                throw new RequestContextException($"Unable to resolve blog from domain '{domain}'");

            contextAccessor.RequestContext = new RequestContext(blog);
            await _next(httpContext);
        }
    }
}
