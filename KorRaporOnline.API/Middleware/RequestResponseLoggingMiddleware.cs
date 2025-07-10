using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KorRaporOnline.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log request
            var requestTime = DateTime.UtcNow;
            _logger.LogInformation(
                "Request {method} {url} started at {requestTime}",
                context.Request.Method,
                context.Request.Path,
                requestTime
            );

            // Call next middleware
            await _next(context);

            // Log response
            var responseTime = DateTime.UtcNow;
            var duration = responseTime - requestTime;
            _logger.LogInformation(
                "Response {statusCode} for {method} {url} completed in {duration}ms",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path,
                duration.TotalMilliseconds
            );
        }
    }
}