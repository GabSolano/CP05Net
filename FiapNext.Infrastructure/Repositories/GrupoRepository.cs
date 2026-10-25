using FiapNext.Domain.Entities;
using FiapNext.Domain.Interfaces;
using FiapNext.Infrastructure.Context;
using MongoDB.Driver;

namespace FiapNext.Infrastructure.Repositories;

public class GrupoRepository : IGrupoRepository
{
    private readonly MongoDbContext _context;

    public GrupoRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Grupo?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Grupos.Find(g => g.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Grupo>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Grupos.Find(_ => true).ToListAsync(ct);
    }

    public async Task CreateAsync(Grupo grupo, CancellationToken ct = default)
    {
        await _context.Grupos.InsertOneAsync(grupo, cancellationToken: ct);
    }

    public async Task UpdateAsync(Grupo grupo, CancellationToken ct = default)
    {
        await _context.Grupos.ReplaceOneAsync(g => g.Id == grupo.Id, grupo, cancellationToken: ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _context.Grupos.DeleteOneAsync(g => g.Id == id, ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Grupos.Find(g => g.Id == id).AnyAsync(ct);
    }
}
