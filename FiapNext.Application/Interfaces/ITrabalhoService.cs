using FiapNext.Application.DTOs;

namespace FiapNext.Application.Interfaces;

public interface ITrabalhoService
{
    Task<TrabalhoResponse> CreateAsync(CreateTrabalhoRequest request, CancellationToken ct = default);
    Task<TrabalhoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<TrabalhoResponse> Items, long TotalCount)> GetPagedAsync(TrabalhoFilterParams filter, CancellationToken ct = default);
    Task<TrabalhoResponse> UpdateAsync(Guid id, UpdateTrabalhoRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
