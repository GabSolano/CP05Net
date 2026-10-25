using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FiapNext.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Autenticação (JWT & Perfis)");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var response = await authService.LoginAsync(request, ct);
            return Results.Ok(response);
        })
        .WithName("Login")
        .WithSummary("Autenticação e emissão de tokens JWT para perfis Admin e Usuario")
        .WithDescription("Autentica usuários (Admin ou Usuario/Aluno) e retorna o token JWT Bearer com as respectivas Claims.")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        return app;
    }
}
