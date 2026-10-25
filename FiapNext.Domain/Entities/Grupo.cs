using FiapNext.Domain.Exceptions;

namespace FiapNext.Domain.Entities;

public class Grupo
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Turma { get; private set; } = string.Empty;
    public List<string> Integrantes { get; private set; } = new();
    public DateTime DataCriacao { get; private set; }

    // Construtor sem parâmetros para serialização/deserialização
    protected Grupo() { }

    public Grupo(string nome, string turma, IEnumerable<string> integrantes, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
        Update(nome, turma, integrantes);
    }

    public void Update(string nome, string turma, IEnumerable<string> integrantes)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 2)
            throw new DomainValidationException("O nome do grupo é obrigatório e deve conter ao menos 2 caracteres.");

        if (string.IsNullOrWhiteSpace(turma))
            throw new DomainValidationException("A turma/classe é obrigatória (ex: 2TDSPV).");

        var listaIntegrantes = integrantes?.Select(i => i.Trim()).Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
        if (listaIntegrantes == null || !listaIntegrantes.Any())
            throw new DomainValidationException("O grupo deve possuir pelo menos um integrante válido.");

        Nome = nome.Trim();
        Turma = turma.Trim();
        Integrantes = listaIntegrantes;
    }
}
