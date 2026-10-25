using FiapNext.Domain.Entities;

namespace FiapNext.Domain.Interfaces;

public interface ITrabalhoRepository
{
    Task<Trabalho?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Trabalho> Items, long TotalCount)> GetPagedAsync(
        string? tituloRegex,
        decimal? minNota,
        decimal? maxNota,
        decimal? minRelevancia,
        decimal? maxRelevancia,
        string? sortBy,
        bool ascending,
        int skip,
        int limit,
        CancellationToken ct = default);
    Task CreateAsync(Trabalho trabalho, CancellationToken ct = default);
    Task UpdateAsync(Trabalho trabalho, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
