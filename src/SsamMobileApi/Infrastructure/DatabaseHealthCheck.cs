using System.Diagnostics;
using SsamMobileApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SsamMobileApi.Infrastructure;

/// <summary>
/// Confirms the API can actually reach the SQL database by opening a connection
/// and running "SELECT 1". Reports the round-trip time and the server/database
/// names so a failed deployment is easy to diagnose.
/// </summary>
public sealed class DatabaseHealthCheck(AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            sw.Stop();

            var conn = db.Database.GetDbConnection();
            var data = new Dictionary<string, object>
            {
                ["server"] = conn.DataSource ?? "unknown",
                ["database"] = conn.Database ?? "unknown",
                ["responseMs"] = sw.ElapsedMilliseconds
            };

            return HealthCheckResult.Healthy("Database connection OK.", data);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return HealthCheckResult.Unhealthy(
                $"Database connection failed after {sw.ElapsedMilliseconds} ms.", ex);
        }
    }
}
