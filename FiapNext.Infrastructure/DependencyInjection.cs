using FiapNext.Application.Interfaces;
using FiapNext.Domain.Interfaces;
using FiapNext.Infrastructure.Authentication;
using FiapNext.Infrastructure.Context;
using FiapNext.Infrastructure.Health;
using FiapNext.Infrastructure.Repositories;
using FiapNext.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapNext.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IGrupoRepository, GrupoRepository>();
        services.AddScoped<ITrabalhoRepository, TrabalhoRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddHealthChecks()
            .AddCheck<MongoHealthCheck>("mongodb_atlas");

        return services;
    }
}
