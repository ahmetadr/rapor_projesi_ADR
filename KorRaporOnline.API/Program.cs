using KorRaporOnline.API.Data;
using KorRaporOnline.API.Extensions;
using KorRaporOnline.API.Filters;
using KorRaporOnline.API.Middleware;
using KorRaporOnline.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuration validation
var connectionString = builder.Configuration.GetConnectionString("KorRaporOnlineConnection")
    ?? throw new InvalidOperationException("Connection string 'KorRaporOnlineConnection' not found.");

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT Settings not configured.");

// Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Application Services
builder.Services
    .AddApplicationServices()
    .AddJwtAuthentication(jwtSettings)
    .AddSwaggerServices()
    .AddHealthCheckServices(builder.Configuration);

// CORS Configuration
var allowedOrigins = builder.Configuration
    .GetSection($"AllowedOrigins:{builder.Environment.EnvironmentName}")
    .Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowCredentials()
              .WithHeaders("Authorization", "Content-Type")
              .WithMethods("GET", "POST", "PUT", "DELETE");
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiLimiter", limiter =>
    {
        limiter.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit", 100);
        limiter.Window = TimeSpan.FromMinutes(
            builder.Configuration.GetValue<int>("RateLimiting:WindowMinutes", 1));
        limiter.QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit", 10);
    });

    options.AddFixedWindowLimiter("LoginLimiter", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(15);
        limiter.QueueLimit = 0;
    });
});

// Controllers Configuration
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
})
.AddNewtonsoftJson()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var result = new BadRequestObjectResult(new
        {
            Success = false,
            Message = "Validation failed",
            Errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
        });
        return result;
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy =>
        policy.RequireRole("User"));
});

var app = builder.Build();

// Development environment settings
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

// CORS middleware should be before Authentication
app.UseCors("CorsPolicy");

// Security middlewares
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.UseHealthChecks();

// Response caching should be after Authorization
app.UseMiddleware<ResponseCacheMiddleware>();

app.MapControllers().RequireAuthorization();

app.Run();

// Required for integration tests
public partial class Program { }