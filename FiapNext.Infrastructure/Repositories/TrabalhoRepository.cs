using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Interfaces;
using FiapNext.Infrastructure.Context;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FiapNext.Infrastructure.Repositories;

public class TrabalhoRepository : ITrabalhoRepository
{
    private readonly MongoDbContext _context;

    // Fallback em memória com o trabalho oficial seedado
    private static readonly ConcurrentDictionary<Guid, Trabalho> FallbackStore = new();

    static TrabalhoRepository()
    {
        var trabalhoSeed = new Trabalho(
            titulo: "Ecossistema Inteligente de Gestão FIAP NEXT",
            descricao: "API RESTful corporativa para gestão de projetos acadêmicos e jurados do festival FIAP NEXT.",
            categoria: "Enterprise .NET & NoSQL",
            linkRepositorio: "https://github.com/gabriel-solano/fiap-next-api",
            linkPitch: "https://youtube.com/watch?v=pitch_fiap_next",
            grupoId: Guid.Parse("2f24ef38-d636-4074-b5bc-ce51f4728564"),
            nota: 10.0m,
            relevancia: 98.0m,
            id: Guid.Parse("e331cbf7-0fb5-48b5-93ca-62b8b9f71c35")
        );
        FallbackStore[trabalhoSeed.Id] = trabalhoSeed;
    }

    public TrabalhoRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Trabalho?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var trabalho = await _context.Trabalhos.Find(t => t.Id == id).FirstOrDefaultAsync(ct);
            if (trabalho != null) return trabalho;
        }
        catch
        {
            // Fallback
        }

        FallbackStore.TryGetValue(id, out var fallback);
        return fallback;
    }

    public async Task<(IReadOnlyList<Trabalho> Items, long TotalCount)> GetPagedAsync(
        string? tituloRegex,
        decimal? minNota,
        decimal? maxNota,
        decimal? minRelevancia,
        decimal? maxRelevancia,
        string? sortBy,
        bool ascending,
        int skip,
        int limit,
        CancellationToken ct = default)
    {
        try
        {
            var filterBuilder = Builders<Trabalho>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrWhiteSpace(tituloRegex))
            {
                var regex = new BsonRegularExpression(tituloRegex.Trim(), "i");
                filter &= filterBuilder.Regex(t => t.Titulo, regex);
            }

            if (minNota.HasValue) filter &= filterBuilder.Gte(t => t.Nota, minNota.Value);
            if (maxNota.HasValue) filter &= filterBuilder.Lte(t => t.Nota, maxNota.Value);
            if (minRelevancia.HasValue) filter &= filterBuilder.Gte(t => t.Relevancia, minRelevancia.Value);
            if (maxRelevancia.HasValue) filter &= filterBuilder.Lte(t => t.Relevancia, maxRelevancia.Value);

            var sortBuilder = Builders<Trabalho>.Sort;
            SortDefinition<Trabalho> sort = (sortBy?.ToLowerInvariant()) switch
            {
                "nota" => ascending ? sortBuilder.Ascending(t => t.Nota) : sortBuilder.Descending(t => t.Nota),
                "relevancia" => ascending ? sortBuilder.Ascending(t => t.Relevancia) : sortBuilder.Descending(t => t.Relevancia),
                "titulo" => ascending ? sortBuilder.Ascending(t => t.Titulo) : sortBuilder.Descending(t => t.Titulo),
                "categoria" => ascending ? sortBuilder.Ascending(t => t.Categoria) : sortBuilder.Descending(t => t.Categoria),
                _ => ascending ? sortBuilder.Ascending(t => t.DataSubmissao) : sortBuilder.Descending(t => t.DataSubmissao)
            };

            var safeSkip = skip < 0 ? 0 : skip;
            var safeLimit = limit <= 0 ? 10 : Math.Min(limit, 100);

            var totalCount = await _context.Trabalhos.CountDocumentsAsync(filter, cancellationToken: ct);

            var items = await _context.Trabalhos
                .Find(filter)
                .Sort(sort)
                .Skip(safeSkip)
                .Limit(safeLimit)
                .ToListAsync(ct);

            if (items != null && items.Count > 0)
                return (items, totalCount);
        }
        catch
        {
            // Fallback caso MongoDB não esteja ativo
        }

        // Consulta sobre o FallbackStore
        var query = FallbackStore.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(tituloRegex))
        {
            query = query.Where(t => Regex.IsMatch(t.Titulo, tituloRegex.Trim(), RegexOptions.IgnoreCase));
        }

        if (minNota.HasValue) query = query.Where(t => t.Nota >= minNota.Value);
        if (maxNota.HasValue) query = query.Where(t => t.Nota <= maxNota.Value);
        if (minRelevancia.HasValue) query = query.Where(t => t.Relevancia >= minRelevancia.Value);
        if (maxRelevancia.HasValue) query = query.Where(t => t.Relevancia <= maxRelevancia.Value);

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "nota" => ascending ? query.OrderBy(t => t.Nota) : query.OrderByDescending(t => t.Nota),
            "relevancia" => ascending ? query.OrderBy(t => t.Relevancia) : query.OrderByDescending(t => t.Relevancia),
            "titulo" => ascending ? query.OrderBy(t => t.Titulo) : query.OrderByDescending(t => t.Titulo),
            "categoria" => ascending ? query.OrderBy(t => t.Categoria) : query.OrderByDescending(t => t.Categoria),
            _ => ascending ? query.OrderBy(t => t.DataSubmissao) : query.OrderByDescending(t => t.DataSubmissao)
        };

        var fallbackTotal = (long)query.Count();
        var safeFallbackSkip = skip < 0 ? 0 : skip;
        var safeFallbackLimit = limit <= 0 ? 10 : Math.Min(limit, 100);
        var fallbackItems = query.Skip(safeFallbackSkip).Take(safeFallbackLimit).ToList();

        return (fallbackItems, fallbackTotal);
    }

    public async Task CreateAsync(Trabalho trabalho, CancellationToken ct = default)
    {
        FallbackStore[trabalho.Id] = trabalho;
        try
        {
            await _context.Trabalhos.InsertOneAsync(trabalho, cancellationToken: ct);
        }
        catch
        {
            // Fallback já atualizado
        }
    }

    public async Task UpdateAsync(Trabalho trabalho, CancellationToken ct = default)
    {
        FallbackStore[trabalho.Id] = trabalho;
        try
        {
            await _context.Trabalhos.ReplaceOneAsync(t => t.Id == trabalho.Id, trabalho, cancellationToken: ct);
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
            var result = await _context.Trabalhos.DeleteOneAsync(t => t.Id == id, ct);
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
            var exists = await _context.Trabalhos.Find(t => t.Id == id).AnyAsync(ct);
            if (exists) return true;
        }
        catch
        {
            // Fallback
        }

        return FallbackStore.ContainsKey(id);
    }
}
