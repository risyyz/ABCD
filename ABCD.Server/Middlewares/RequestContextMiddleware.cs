using ABCD.Data;
using ABCD.Lib;
using ABCD.Lib.Exceptions;

using Microsoft.EntityFrameworkCore;

namespace ABCD.Server.Middlewares {
    public class RequestContextMiddleware {
        private readonly RequestDelegate _next;

        public RequestContextMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, DataContext dataContext, RequestContextAccessor contextAccessor) {
            //TODO resolve from cache

            var domain = httpContext.Request.Host.Host;
            var blog = await (
                from d in dataContext.BlogDomains
                join b in dataContext.Blogs on d.BlogId equals b.BlogId
                where d.Domain.ToLowerInvariant() == domain.ToLowerInvariant()
                select b
            ).FirstOrDefaultAsync();

            if (blog == null)
                throw new RequestContextException($"Unable to resolve blog from domain '{domain}'");

            contextAccessor.RequestContext = new RequestContext(blog);
            await _next(httpContext);
        }
    }
}
