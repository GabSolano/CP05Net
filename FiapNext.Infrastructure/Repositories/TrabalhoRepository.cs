using FiapNext.Domain.Entities;
using FiapNext.Domain.Interfaces;
using FiapNext.Infrastructure.Context;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FiapNext.Infrastructure.Repositories;

public class TrabalhoRepository : ITrabalhoRepository
{
    private readonly MongoDbContext _context;

    public TrabalhoRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Trabalho?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Trabalhos.Find(t => t.Id == id).FirstOrDefaultAsync(ct);
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
        var filterBuilder = Builders<Trabalho>.Filter;
        var filter = filterBuilder.Empty;

        // Filtro por nome/título usando Regex case-insensitive
        if (!string.IsNullOrWhiteSpace(tituloRegex))
        {
            var regex = new BsonRegularExpression(tituloRegex.Trim(), "i");
            filter &= filterBuilder.Regex(t => t.Titulo, regex);
        }

        // Filtro por faixa de notas
        if (minNota.HasValue)
        {
            filter &= filterBuilder.Gte(t => t.Nota, minNota.Value);
        }
        if (maxNota.HasValue)
        {
            filter &= filterBuilder.Lte(t => t.Nota, maxNota.Value);
        }

        // Filtro por faixa de relevância
        if (minRelevancia.HasValue)
        {
            filter &= filterBuilder.Gte(t => t.Relevancia, minRelevancia.Value);
        }
        if (maxRelevancia.HasValue)
        {
            filter &= filterBuilder.Lte(t => t.Relevancia, maxRelevancia.Value);
        }

        // Ordenação dinâmica
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

        return (items, totalCount);
    }

    public async Task CreateAsync(Trabalho trabalho, CancellationToken ct = default)
    {
        await _context.Trabalhos.InsertOneAsync(trabalho, cancellationToken: ct);
    }

    public async Task UpdateAsync(Trabalho trabalho, CancellationToken ct = default)
    {
        await _context.Trabalhos.ReplaceOneAsync(t => t.Id == trabalho.Id, trabalho, cancellationToken: ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _context.Trabalhos.DeleteOneAsync(t => t.Id == id, ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Trabalhos.Find(t => t.Id == id).AnyAsync(ct);
    }
}
