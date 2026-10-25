using FiapNext.Application.Interfaces;
using FiapNext.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FiapNext.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHateoasService, HateoasService>();
        services.AddScoped<IGrupoService, GrupoService>();
        services.AddScoped<ITrabalhoService, TrabalhoService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
