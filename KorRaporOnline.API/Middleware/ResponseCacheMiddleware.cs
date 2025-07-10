using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KorRaporOnline.API.Middleware
{
    public class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly int _timeToLiveSeconds;

        public ResponseCacheMiddleware(RequestDelegate next, IMemoryCache cache, int timeToLiveSeconds = 30)
        {
            _next = next;
            _cache = cache;
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var cacheKey = $"Response_{path}_{context.User.Identity?.Name ?? "anonymous"}";

            if (context.Request.Method == "GET" && _cache.TryGetValue(cacheKey, out string cachedResponse))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(cachedResponse);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            if (context.Response.StatusCode == 200)
            {
                _cache.Set(cacheKey, responseBody, TimeSpan.FromSeconds(_timeToLiveSeconds));
            }

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
    }
}