using System.Collections.Concurrent;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Interfaces;
using FiapNext.Infrastructure.Context;
using MongoDB.Driver;

namespace FiapNext.Infrastructure.Repositories;

public class GrupoRepository : IGrupoRepository
{
    private readonly MongoDbContext _context;

    // Fallback em memória com o grupo oficial seedado
    private static readonly ConcurrentDictionary<Guid, Grupo> FallbackStore = new();

    static GrupoRepository()
    {
        var grupoSeed = new Grupo(
            nome: "Equipe NEXT 2TDSPV",
            turma: "2TDSPV",
            integrantes: new[]
            {
                "Gabriel Costa Solano (RM562325)",
                "Leandro Guarido (RM561760)",
                "Kaiky Pereira Rodrigues (RM564578)"
            },
            id: Guid.Parse("2f24ef38-d636-4074-b5bc-ce51f4728564")
        );
        FallbackStore[grupoSeed.Id] = grupoSeed;
    }

    public GrupoRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Grupo?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var grupo = await _context.Grupos.Find(g => g.Id == id).FirstOrDefaultAsync(ct);
            if (grupo != null) return grupo;
        }
        catch
        {
            // Fallback
        }

        FallbackStore.TryGetValue(id, out var fallback);
        return fallback;
    }

    public async Task<IReadOnlyList<Grupo>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var list = await _context.Grupos.Find(_ => true).ToListAsync(ct);
            if (list != null && list.Count > 0) return list;
        }
        catch
        {
            // Fallback
        }

        return FallbackStore.Values.ToList();
    }

    public async Task CreateAsync(Grupo grupo, CancellationToken ct = default)
    {
        FallbackStore[grupo.Id] = grupo;
        try
        {
            await _context.Grupos.InsertOneAsync(grupo, cancellationToken: ct);
        }
        catch
        {
            // Fallback já atualizado
        }
    }

    public async Task UpdateAsync(Grupo grupo, CancellationToken ct = default)
    {
        FallbackStore[grupo.Id] = grupo;
        try
        {
            await _context.Grupos.ReplaceOneAsync(g => g.Id == grupo.Id, grupo, cancellationToken: ct);
        }
        catch
        {
            // Fallback já atualizado
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var removed = FallbackStore.TryRemove(id, out _);
        try
        {
            var result = await _context.Grupos.DeleteOneAsync(g => g.Id == id, ct);
            return result.DeletedCount > 0 || removed;
        }
        catch
        {
            return removed;
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var exists = await _context.Grupos.Find(g => g.Id == id).AnyAsync(ct);
            if (exists) return true;
        }
        catch
        {
            // Fallback
        }

        return FallbackStore.ContainsKey(id);
    }
}
