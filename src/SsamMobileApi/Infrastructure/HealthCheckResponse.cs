using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SsamMobileApi.Infrastructure;

/// <summary>
/// Writes health-check results as JSON so callers (you, OutSystems, a monitor)
/// can see which check failed. Full diagnostic detail (server names, exception
/// messages) is only included outside Production to avoid leaking internals.
/// </summary>
public static class HealthCheckResponse
{
    public static HealthCheckOptions Options(bool verbose) => new()
    {
        ResponseWriter = (context, report) => WriteJson(context, report, verbose)
    };

    public static HealthCheckOptions Options(bool verbose, Func<HealthCheckRegistration, bool> predicate) => new()
    {
        Predicate = predicate,
        ResponseWriter = (context, report) => WriteJson(context, report, verbose)
    };

    private static Task WriteJson(HttpContext context, HealthReport report, bool verbose)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),         // Healthy | Degraded | Unhealthy
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = Math.Round(e.Value.Duration.TotalMilliseconds),
                data = verbose && e.Value.Data.Count > 0 ? e.Value.Data : null,
                error = verbose ? e.Value.Exception?.Message : null
            })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }
}
