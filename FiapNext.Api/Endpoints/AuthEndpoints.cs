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

        group.MapPost("/register", async (
            [FromBody] RegisterUserRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var user = await authService.RegisterAsync(request, ct);
            return Results.Created($"/api/auth/users/{user.Id}", user);
        })
        .WithName("RegistrarUsuario")
        .WithSummary("Registro de novos usuários, alunos e consultores públicos")
        .WithDescription("Permite o cadastro aberto de usuários (roles: Usuario, Consultor, Admin) para acesso ao festival.")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        group.MapGet("/users", async (
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var users = await authService.GetAllUsersAsync(ct);
            return Results.Ok(users);
        })
        .WithName("ListarUsuarios")
        .WithSummary("Lista todos os usuários e consultores registrados")
        .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)
        .RequireAuthorization();

        return app;
    }
}
