namespace FiapNext.Application.DTOs;

public record CreateGrupoRequest(
    string Nome,
    string Turma,
    List<string> Integrantes
);

public record UpdateGrupoRequest(
    string Nome,
    string Turma,
    List<string> Integrantes
);

public record GrupoResponse(
    Guid Id,
    string Nome,
    string Turma,
    IReadOnlyList<string> Integrantes,
    DateTime DataCriacao
);
