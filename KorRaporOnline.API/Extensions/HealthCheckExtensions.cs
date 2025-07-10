using KorRaporOnline.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KorRaporOnline.API.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthCheckServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>(
                    "Database",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "db", "sql", "sqlserver" })
                .AddCheck<ApiHealthCheck>(
                    "API",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "api", "service" })
                .AddCheck("Storage", () =>
                {
                    try
                    {
                        // Storage health check logic
                        return HealthCheckResult.Healthy("Storage is accessible");
                    }
                    catch (Exception ex)
                    {
                        return HealthCheckResult.Unhealthy("Storage check failed", ex);
                    }
                });

            return services;
        }

        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(x => new
                        {
                            name = x.Key,
                            status = x.Value.Status.ToString(),
                            duration = x.Value.Duration.ToString(),
                            description = x.Value.Description,
                            data = x.Value.Data
                        }),
                        totalDuration = report.TotalDuration.ToString()
                    };

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    await JsonSerializer.SerializeAsync(context.Response.Body, response, options);
                }
            });

            return app;
        }
    }

    public class ApiHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isHealthy = true; // Gerçek kontroller burada yapılacak

                var data = new Dictionary<string, object>
                {
                    { "LastChecked", DateTime.UtcNow },
                    { "Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") }
                };

                if (isHealthy)
                {
                    return Task.FromResult(
                        HealthCheckResult.Healthy("API is functioning normally", data));
                }

                return Task.FromResult(
                    HealthCheckResult.Degraded("API is functioning with reduced performance", null, data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy("API health check failed", ex));
            }
        }
    }
}