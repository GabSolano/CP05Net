using System.Diagnostics;
using System.Security.Claims;
using FiapNext.Api.Telemetry;
using FiapNext.Application.Common.Hateoas;
using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FiapNext.Api.Endpoints;

public static class TrabalhosEndpoints
{
    public static IEndpointRouteBuilder MapTrabalhosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trabalhos")
            .WithTags("Trabalhos / Projetos FIAP NEXT");

        // RF02 - Cadastro de Trabalhos (Escrita - Somente Admin)
        group.MapPost("/", async (
            [FromBody] CreateTrabalhoRequest request,
            [FromServices] ITrabalhoService trabalhoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var activity = FiapNextTelemetry.ActivitySource.StartActivity("Trabalho.CriarSubmissao");
            activity?.SetTag("trabalho.titulo", request.Titulo);
            activity?.SetTag("trabalho.categoria", request.Categoria);
            activity?.SetTag("trabalho.grupo_id", request.GrupoId.ToString());

            var baseUrl = GetBaseUrl(httpContext);
            var isAdmin = httpContext.User.IsInRole("Admin");

            var created = await trabalhoService.CreateAsync(request, ct);
            var enveloped = hateoasService.EnvelopTrabalho(created, isAdmin, baseUrl);

            activity?.SetTag("trabalho.id", created.Id.ToString());

            return Results.Created($"{baseUrl}/api/trabalhos/{created.Id}", enveloped);
        })
        .WithName("CadastrarTrabalho")
        .WithSummary("Cadastra um novo trabalho do FIAP NEXT (Somente Admin)")
        .WithDescription("Permite que administradores, professores e jurados cadastrem novos projetos no festival.")
        .Produces<ResourceEnvelope<TrabalhoResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization("SomenteAdmin");

        // RF03 - Consulta Paginada e Filtrada (Usuários autenticados)
        group.MapGet("/", async (
            [FromQuery] string? titulo,
            [FromQuery] decimal? minNota,
            [FromQuery] decimal? maxNota,
            [FromQuery] decimal? minRelevancia,
            [FromQuery] decimal? maxRelevancia,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            [FromServices] ITrabalhoService trabalhoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var activity = FiapNextTelemetry.ActivitySource.StartActivity("Trabalho.ConsultarPaginado");
            activity?.SetTag("filtro.titulo", titulo);
            activity?.SetTag("filtro.min_nota", minNota?.ToString());
            activity?.SetTag("filtro.max_nota", maxNota?.ToString());

            var filter = new TrabalhoFilterParams(
                Titulo: titulo,
                MinNota: minNota,
                MaxNota: maxNota,
                MinRelevancia: minRelevancia,
                MaxRelevancia: maxRelevancia,
                SortBy: sortBy,
                SortOrder: sortOrder ?? "asc",
                Skip: skip ?? 0,
                Limit: limit ?? 10
            );

            var baseUrl = GetBaseUrl(httpContext);
            var isAdmin = httpContext.User.IsInRole("Admin");

            var (items, totalCount) = await trabalhoService.GetPagedAsync(filter, ct);
            var result = hateoasService.EnvelopTrabalhosPaged(items, filter, totalCount, isAdmin, baseUrl);

            activity?.SetTag("resultado.total", totalCount);
            activity?.SetTag("resultado.retornados", items.Count);

            return Results.Ok(result);
        })
        .WithName("ListarTrabalhosPaginados")
        .WithSummary("Consulta paginada e filtrada de trabalhos do NEXT com HATEOAS")
        .WithDescription("Suporta regex case-insensitive por título, filtro por faixa de notas/relevância, ordenação dinâmica e paginação com metadados.")
        .Produces<PagedResult<ResourceEnvelope<TrabalhoResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        // RF04 - Consulta por ID (Usuários autenticados)
        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] ITrabalhoService trabalhoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var activity = FiapNextTelemetry.ActivitySource.StartActivity("Trabalho.ConsultarPorId");
            activity?.SetTag("trabalho.id", id.ToString());

            var trabalho = await trabalhoService.GetByIdAsync(id, ct);
            if (trabalho == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Trabalho Não Encontrado",
                    Detail = $"O trabalho com o identificador {id} não foi encontrado no festival NEXT."
                });
            }

            var baseUrl = GetBaseUrl(httpContext);
            var isAdmin = httpContext.User.IsInRole("Admin");
            var enveloped = hateoasService.EnvelopTrabalho(trabalho, isAdmin, baseUrl);

            return Results.Ok(enveloped);
        })
        .WithName("ObterTrabalhoPorId")
        .WithSummary("Detalha um projeto específico por ID com hiperlinks HATEOAS")
        .Produces<ResourceEnvelope<TrabalhoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        // Atualização de Trabalho (Somente Admin - link HATEOAS 'update')
        group.MapPut("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateTrabalhoRequest request,
            [FromServices] ITrabalhoService trabalhoService,
            [FromServices] IHateoasService hateoasService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            using var activity = FiapNextTelemetry.ActivitySource.StartActivity("Trabalho.Atualizar");
            activity?.SetTag("trabalho.id", id.ToString());

            var updated = await trabalhoService.UpdateAsync(id, request, ct);
            var baseUrl = GetBaseUrl(httpContext);
            var enveloped = hateoasService.EnvelopTrabalho(updated, true, baseUrl);

            return Results.Ok(enveloped);
        })
        .WithName("AtualizarTrabalho")
        .WithSummary("Atualiza os dados de um trabalho existente (Somente Admin)")
        .Produces<ResourceEnvelope<TrabalhoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization("SomenteAdmin");

        // Exclusão de Trabalho (Somente Admin - link HATEOAS 'delete')
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] ITrabalhoService trabalhoService,
            CancellationToken ct) =>
        {
            using var activity = FiapNextTelemetry.ActivitySource.StartActivity("Trabalho.Deletar");
            activity?.SetTag("trabalho.id", id.ToString());

            var deleted = await trabalhoService.DeleteAsync(id, ct);
            if (!deleted)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Trabalho Não Encontrado",
                    Detail = $"Não foi possível excluir o trabalho com id {id} pois ele não existe."
                });
            }

            return Results.NoContent();
        })
        .WithName("ExcluirTrabalho")
        .WithSummary("Remove um trabalho submetido (Somente Admin)")
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
