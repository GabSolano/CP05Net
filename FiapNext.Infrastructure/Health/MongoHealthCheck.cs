using FiapNext.Infrastructure.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;

namespace FiapNext.Infrastructure.Health;

public class MongoHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _context;

    public MongoHealthCheck(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Executa comando ping no MongoDB Atlas
            var pingCommand = new BsonDocument("ping", 1);
            await _context.Database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Conexão com MongoDB Atlas operacional e responsiva.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Falha na conexão com MongoDB Atlas: {ex.Message}", ex);
        }
    }
}
