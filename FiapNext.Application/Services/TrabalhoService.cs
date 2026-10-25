using FiapNext.Application.Common.Metrics;
using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Exceptions;
using FiapNext.Domain.Interfaces;

namespace FiapNext.Application.Services;

public class TrabalhoService : ITrabalhoService
{
    private readonly ITrabalhoRepository _trabalhoRepository;
    private readonly IGrupoRepository _grupoRepository;
    private readonly IFiapNextMetrics? _metrics;

    public TrabalhoService(
        ITrabalhoRepository trabalhoRepository,
        IGrupoRepository grupoRepository,
        IFiapNextMetrics? metrics = null)
    {
        _trabalhoRepository = trabalhoRepository;
        _grupoRepository = grupoRepository;
        _metrics = metrics;
    }

    public async Task<TrabalhoResponse> CreateAsync(CreateTrabalhoRequest request, CancellationToken ct = default)
    {
        var grupo = await _grupoRepository.GetByIdAsync(request.GrupoId, ct);
        if (grupo == null)
            throw new DomainValidationException($"Grupo informado ({request.GrupoId}) não foi encontrado.");

        var trabalho = new Trabalho(
            request.Titulo,
            request.Descricao,
            request.Categoria,
            request.LinkRepositorio,
            request.LinkPitch,
            request.GrupoId,
            request.Nota ?? 0,
            request.Relevancia ?? 0
        );

        trabalho.AssociarGrupo(grupo);
        await _trabalhoRepository.CreateAsync(trabalho, ct);

        _metrics?.RecordTrabalhoSubmetido(trabalho.Categoria);

        return MapToResponse(trabalho);
    }

    public async Task<TrabalhoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var trabalho = await _trabalhoRepository.GetByIdAsync(id, ct);
        if (trabalho == null) return null;

        if (trabalho.Grupo == null && trabalho.GrupoId != Guid.Empty)
        {
            var grupo = await _grupoRepository.GetByIdAsync(trabalho.GrupoId, ct);
            if (grupo != null)
            {
                trabalho.AssociarGrupo(grupo);
            }
        }

        return MapToResponse(trabalho);
    }

    public async Task<(IReadOnlyList<TrabalhoResponse> Items, long TotalCount)> GetPagedAsync(
        TrabalhoFilterParams filter,
        CancellationToken ct = default)
    {
        var isAscending = string.IsNullOrWhiteSpace(filter.SortOrder) ||
                          !filter.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        var (items, totalCount) = await _trabalhoRepository.GetPagedAsync(
            filter.Titulo,
            filter.MinNota,
            filter.MaxNota,
            filter.MinRelevancia,
            filter.MaxRelevancia,
            filter.SortBy,
            isAscending,
            filter.Skip,
            filter.Limit,
            ct
        );

        // Carregar nomes de grupo se não estiverem populados
        var grupoIds = items.Where(t => t.Grupo == null).Select(t => t.GrupoId).Distinct().ToList();
        var gruposCache = new Dictionary<Guid, Grupo>();
        foreach (var gId in grupoIds)
        {
            var g = await _grupoRepository.GetByIdAsync(gId, ct);
            if (g != null) gruposCache[gId] = g;
        }

        var responseList = items.Select(t =>
        {
            if (t.Grupo == null && gruposCache.TryGetValue(t.GrupoId, out var g))
            {
                t.AssociarGrupo(g);
            }
            return MapToResponse(t);
        }).ToList();

        return (responseList, totalCount);
    }

    public async Task<TrabalhoResponse> UpdateAsync(Guid id, UpdateTrabalhoRequest request, CancellationToken ct = default)
    {
        var trabalho = await _trabalhoRepository.GetByIdAsync(id, ct);
        if (trabalho == null)
            throw new KeyNotFoundException($"Trabalho com ID {id} não foi encontrado.");

        trabalho.Update(
            request.Titulo,
            request.Descricao,
            request.Categoria,
            request.LinkRepositorio,
            request.LinkPitch,
            request.Nota ?? trabalho.Nota,
            request.Relevancia ?? trabalho.Relevancia
        );

        await _trabalhoRepository.UpdateAsync(trabalho, ct);
        return MapToResponse(trabalho);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _trabalhoRepository.DeleteAsync(id, ct);
    }

    private static TrabalhoResponse MapToResponse(Trabalho trabalho)
    {
        return new TrabalhoResponse(
            trabalho.Id,
            trabalho.Titulo,
            trabalho.Descricao,
            trabalho.Categoria,
            trabalho.LinkRepositorio,
            trabalho.LinkPitch,
            trabalho.GrupoId,
            trabalho.Grupo?.Nome,
            trabalho.Nota,
            trabalho.Relevancia,
            trabalho.DataSubmissao
        );
    }
}
