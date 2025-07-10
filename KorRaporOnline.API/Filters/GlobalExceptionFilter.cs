using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using KorRaporOnline.API.Models;

namespace KorRaporOnline.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception occurred");

            var errorResponse = new ErrorResponse
            {
                TraceId = Guid.NewGuid().ToString(),
                Message = _env.IsDevelopment() 
                    ? context.Exception.Message 
                    : "An unexpected error occurred",
                TimeStamp = DateTime.UtcNow
            };

            if (_env.IsDevelopment())
            {
                errorResponse.Details = context.Exception.StackTrace;
            }

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}