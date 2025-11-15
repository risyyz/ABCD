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
                var blogId = await dataContext.Set<BlogDomain>()
                    .Where(d => d.Domain == new Domain(domain))
                    .Select(d => d.BlogId)
                    .FirstOrDefaultAsync();

                if(blogId != 0) {
                    blog = await dataContext.Set<Blog>()
                        .Include(b => EF.Property<ICollection<BlogDomain>>(b, "_domains"))
                        .FirstOrDefaultAsync(b => b.BlogId == blogId);
                }
            }

            if (blog == null)
                throw new RequestContextException($"Unable to resolve blog from domain '{domain}'");

            _cache.Set(domain, blog, TimeSpan.FromMinutes(_cachingSettings.DomainCacheDurationInMinutes));
            contextAccessor.RequestContext = new RequestContext(blog);
            await _next(httpContext);
        }
    }
}
