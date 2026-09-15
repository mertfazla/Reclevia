using Microsoft.Extensions.Diagnostics.HealthChecks;
using Reclevia.Api.Persistence;

namespace Reclevia.Api.Health;

public class DatabaseHealthCheck: IHealthCheck
{
    private readonly RecleviaDbContext _dbContext;

    public DatabaseHealthCheck(RecleviaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("PostgreSql is unavailable.");
    }
}
