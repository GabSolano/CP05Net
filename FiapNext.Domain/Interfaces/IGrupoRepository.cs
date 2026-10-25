using FiapNext.Domain.Entities;

namespace FiapNext.Domain.Interfaces;

public interface IGrupoRepository
{
    Task<Grupo?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Grupo>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(Grupo grupo, CancellationToken ct = default);
    Task UpdateAsync(Grupo grupo, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
