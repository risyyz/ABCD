using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ABCD.Lib.Exceptions; // Ensure this namespace is included

namespace ABCD.Server.Middlewares {
    public class ExceptionHandlingMiddleware {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

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
            int statusCode;
            string errorMessage = exception.Message;

            switch (exception) {
                case SignInFailedException:
                case RequestContextException:
                case BlogNotFoundException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    errorMessage = "An unexpected error occurred.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new {
                error = errorMessage
            });

            return context.Response.WriteAsync(result);
        }
    }
}
