using FiapNext.Api.Telemetry;
using FiapNext.Application.Common.Hateoas;
using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FiapNext.Api.Endpoints;

public static class GruposEndpoints
{
    public static IEndpointRouteBuilder MapGruposEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/grupos")
            .WithTags("Grupos de Alunos");

        // Cadastro de Grupo (Somente Admin)
        group.MapPost("/", async (
            [FromBody] CreateGrupoRequest request,
            [FromServices] IGrupoService grupoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var activity = FiapNextTelemetry.ActivitySource.StartActivity("Grupo.Criar");
            activity?.SetTag("grupo.nome", request.Nome);
            activity?.SetTag("grupo.turma", request.Turma);

            var baseUrl = GetBaseUrl(httpContext);
            var isAdmin = httpContext.User.IsInRole("Admin");

            var created = await grupoService.CreateAsync(request, ct);
            var enveloped = hateoasService.EnvelopGrupo(created, isAdmin, baseUrl);

            return Results.Created($"{baseUrl}/api/grupos/{created.Id}", enveloped);
        })
        .WithName("CadastrarGrupo")
        .WithSummary("Cadastra uma nova equipe de estudantes (Somente Admin)")
        .Produces<ResourceEnvelope<GrupoResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization("SomenteAdmin");

        // Listagem de Grupos (Autenticado)
        group.MapGet("/", async (
            [FromServices] IGrupoService grupoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var baseUrl = GetBaseUrl(httpContext);
            var isAdmin = httpContext.User.IsInRole("Admin");

            var grupos = await grupoService.GetAllAsync(ct);
            var enveloped = grupos.Select(g => hateoasService.EnvelopGrupo(g, isAdmin, baseUrl)).ToList();

            return Results.Ok(enveloped);
        })
        .WithName("ListarGrupos")
        .WithSummary("Lista todos os grupos participantes cadastrados")
        .Produces<IReadOnlyList<ResourceEnvelope<GrupoResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        // Consulta de Grupo por ID (Autenticado)
        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IGrupoService grupoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var grupo = await grupoService.GetByIdAsync(id, ct);
            if (grupo == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Grupo Não Encontrado",
                    Detail = $"O grupo com o identificador {id} não foi encontrado."
                });
            }

            var baseUrl = GetBaseUrl(httpContext);
            var isAdmin = httpContext.User.IsInRole("Admin");
            var enveloped = hateoasService.EnvelopGrupo(grupo, isAdmin, baseUrl);

            return Results.Ok(enveloped);
        })
        .WithName("ObterGrupoPorId")
        .WithSummary("Obtém os detalhes de um grupo por ID")
        .Produces<ResourceEnvelope<GrupoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        // Atualização de Grupo (Somente Admin)
        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateGrupoRequest request,
            [FromServices] IGrupoService grupoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var updated = await grupoService.UpdateAsync(id, request, ct);
            var baseUrl = GetBaseUrl(httpContext);
            var enveloped = hateoasService.EnvelopGrupo(updated, true, baseUrl);

            return Results.Ok(enveloped);
        })
        .WithName("AtualizarGrupo")
        .WithSummary("Atualiza os dados de um grupo (Somente Admin)")
        .Produces<ResourceEnvelope<GrupoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization("SomenteAdmin");

        // Exclusão de Grupo (Somente Admin)
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IGrupoService grupoService,
            CancellationToken ct) =>
        {
            var deleted = await grupoService.DeleteAsync(id, ct);
            if (!deleted)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Grupo Não Encontrado",
                    Detail = $"Não foi possível excluir o grupo com id {id} pois ele não existe."
                });
            }

            return Results.NoContent();
        })
        .WithName("ExcluirGrupo")
        .WithSummary("Remove um grupo de alunos (Somente Admin)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization("SomenteAdmin");

        return app;
    }

    private static string GetBaseUrl(HttpContext context)
    {
        return $"{context.Request.Scheme}://{context.Request.Host.Value}";
    }
}
