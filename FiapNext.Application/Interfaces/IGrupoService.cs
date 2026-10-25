using FiapNext.Application.DTOs;

namespace FiapNext.Application.Interfaces;

public interface IGrupoService
{
    Task<GrupoResponse> CreateAsync(CreateGrupoRequest request, CancellationToken ct = default);
    Task<GrupoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<GrupoResponse>> GetAllAsync(CancellationToken ct = default);
    Task<GrupoResponse> UpdateAsync(Guid id, UpdateGrupoRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
