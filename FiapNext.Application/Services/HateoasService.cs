using FiapNext.Application.Common.Hateoas;
using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;

namespace FiapNext.Application.Services;

public class HateoasService : IHateoasService
{
    public ResourceEnvelope<TrabalhoResponse> EnvelopTrabalho(TrabalhoResponse trabalho, bool isAdmin, string baseUrl)
    {
        var cleanBase = baseUrl.TrimEnd('/');
        var links = new List<LinkDto>
        {
            new($"{cleanBase}/api/trabalhos/{trabalho.Id}", "self", "GET"),
            new($"{cleanBase}/api/grupos/{trabalho.GrupoId}", "grupo", "GET"),
            new($"{cleanBase}/api/trabalhos", "collection", "GET")
        };

        // Regra de Negócio HATEOAS: Links de alteração/exclusão apenas para perfil Admin
        if (isAdmin)
        {
            links.Add(new($"{cleanBase}/api/trabalhos/{trabalho.Id}", "update", "PUT"));
            links.Add(new($"{cleanBase}/api/trabalhos/{trabalho.Id}", "delete", "DELETE"));
        }

        return new ResourceEnvelope<TrabalhoResponse>(trabalho, links);
    }

    public PagedResult<ResourceEnvelope<TrabalhoResponse>> EnvelopTrabalhosPaged(
        IReadOnlyList<TrabalhoResponse> itens,
        TrabalhoFilterParams filter,
        long totalItems,
        bool isAdmin,
        string baseUrl)
    {
        var cleanBase = baseUrl.TrimEnd('/');
        var limit = filter.Limit <= 0 ? 10 : filter.Limit;
        var skip = filter.Skip < 0 ? 0 : filter.Skip;
        var totalPages = (int)Math.Ceiling((double)totalItems / limit);
        var currentPage = (skip / limit) + 1;

        var envelopedItems = itens.Select(t => EnvelopTrabalho(t, isAdmin, baseUrl)).ToList();

        var links = new List<LinkDto>
        {
            new(BuildPagedUrl(cleanBase, filter, skip, limit), "self", "GET")
        };

        if (isAdmin)
        {
            links.Add(new($"{cleanBase}/api/trabalhos", "create", "POST"));
        }

        if (currentPage > 1)
        {
            links.Add(new(BuildPagedUrl(cleanBase, filter, 0, limit), "first", "GET"));
            var prevSkip = Math.Max(0, skip - limit);
            links.Add(new(BuildPagedUrl(cleanBase, filter, prevSkip, limit), "prev", "GET"));
        }

        if (currentPage < totalPages)
        {
            var nextSkip = skip + limit;
            links.Add(new(BuildPagedUrl(cleanBase, filter, nextSkip, limit), "next", "GET"));
            var lastSkip = Math.Max(0, (totalPages - 1) * limit);
            links.Add(new(BuildPagedUrl(cleanBase, filter, lastSkip, limit), "last", "GET"));
        }

        return new PagedResult<ResourceEnvelope<TrabalhoResponse>>(
            envelopedItems,
            skip,
            limit,
            totalItems,
            totalPages,
            currentPage,
            links
        );
    }

    public ResourceEnvelope<GrupoResponse> EnvelopGrupo(GrupoResponse grupo, bool isAdmin, string baseUrl)
    {
        var cleanBase = baseUrl.TrimEnd('/');
        var links = new List<LinkDto>
        {
            new($"{cleanBase}/api/grupos/{grupo.Id}", "self", "GET"),
            new($"{cleanBase}/api/grupos", "collection", "GET")
        };

        if (isAdmin)
        {
            links.Add(new($"{cleanBase}/api/grupos/{grupo.Id}", "update", "PUT"));
            links.Add(new($"{cleanBase}/api/grupos/{grupo.Id}", "delete", "DELETE"));
        }

        return new ResourceEnvelope<GrupoResponse>(grupo, links);
    }

    private static string BuildPagedUrl(string baseUrl, TrabalhoFilterParams filter, int skip, int limit)
    {
        var queryParams = new List<string>
        {
            $"skip={skip}",
            $"limit={limit}"
        };

        if (!string.IsNullOrWhiteSpace(filter.Titulo))
            queryParams.Add($"titulo={Uri.EscapeDataString(filter.Titulo)}");

        if (filter.MinNota.HasValue)
            queryParams.Add($"minNota={filter.MinNota.Value}");

        if (filter.MaxNota.HasValue)
            queryParams.Add($"maxNota={filter.MaxNota.Value}");

        if (filter.MinRelevancia.HasValue)
            queryParams.Add($"minRelevancia={filter.MinRelevancia.Value}");

        if (filter.MaxRelevancia.HasValue)
            queryParams.Add($"maxRelevancia={filter.MaxRelevancia.Value}");

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
            queryParams.Add($"sortBy={Uri.EscapeDataString(filter.SortBy)}");

        if (!string.IsNullOrWhiteSpace(filter.SortOrder))
            queryParams.Add($"sortOrder={Uri.EscapeDataString(filter.SortOrder)}");

        return $"{baseUrl}/api/trabalhos?{string.Join("&", queryParams)}";
    }
}
