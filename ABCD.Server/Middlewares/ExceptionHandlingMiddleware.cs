using System.Text.Json;

using ABCD.Application.Exceptions;
using ABCD.Domain.Exceptions;

namespace ABCD.Server.Middlewares {
    public class ExceptionHandlingMiddleware {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly Dictionary<Type, int> ExceptionStatusCodes = new()
        {
            { typeof(IllegalOperationException), StatusCodes.Status400BadRequest },
            { typeof(InvalidFragmentException), StatusCodes.Status400BadRequest },
            { typeof(DuplicatePathSegmentException), StatusCodes.Status400BadRequest },
            { typeof(DuplicatePostTitleException), StatusCodes.Status400BadRequest },

            { typeof(RequestContextException), StatusCodes.Status401Unauthorized },
            { typeof(SignInFailedException), StatusCodes.Status401Unauthorized },            

            { typeof(BlogNotFoundException), StatusCodes.Status404NotFound }
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger) {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context) {
            try {
                await _next(context);
            } catch (Exception ex) {
                _logger.LogError(ex, "Unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception) {
            int statusCode = ExceptionStatusCodes.TryGetValue(exception.GetType(), out var code) ? code : StatusCodes.Status500InternalServerError;
            string errorMessage = statusCode == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            var result = JsonSerializer.Serialize(new { error = errorMessage });
            return context.Response.WriteAsync(result);
        }
    }
}
