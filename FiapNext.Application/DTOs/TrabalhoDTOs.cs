namespace FiapNext.Application.DTOs;

public record CreateTrabalhoRequest(
    string Titulo,
    string Descricao,
    string Categoria,
    string LinkRepositorio,
    string LinkPitch,
    Guid GrupoId,
    decimal? Nota,
    decimal? Relevancia
);

public record UpdateTrabalhoRequest(
    string Titulo,
    string Descricao,
    string Categoria,
    string LinkRepositorio,
    string LinkPitch,
    decimal? Nota,
    decimal? Relevancia
);

public record TrabalhoResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    string Categoria,
    string LinkRepositorio,
    string LinkPitch,
    Guid GrupoId,
    string? GrupoNome,
    decimal Nota,
    decimal Relevancia,
    DateTime DataSubmissao
);

public record TrabalhoFilterParams(
    string? Titulo = null,
    decimal? MinNota = null,
    decimal? MaxNota = null,
    decimal? MinRelevancia = null,
    decimal? MaxRelevancia = null,
    string? SortBy = null,
    string? SortOrder = "asc",
    int Skip = 0,
    int Limit = 10
);
