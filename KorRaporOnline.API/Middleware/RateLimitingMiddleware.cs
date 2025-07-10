using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KorRaporOnline.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new();
        private static readonly TimeSpan _requestInterval = TimeSpan.FromSeconds(1);

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.ToString();
            var key = $"{ip}:{path}";

            if (_lastRequestTimes.TryGetValue(key, out var lastRequestTime))
            {
                var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
                if (timeSinceLastRequest < _requestInterval)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                    return;
                }
            }

            _lastRequestTimes.AddOrUpdate(key, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
            await _next(context);
        }
    }
}