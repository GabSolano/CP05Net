using FiapNext.Domain.Exceptions;

namespace FiapNext.Domain.Entities;

public class Trabalho
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;
    public string LinkRepositorio { get; private set; } = string.Empty;
    public string LinkPitch { get; private set; } = string.Empty;
    public Guid GrupoId { get; private set; }
    public Grupo? Grupo { get; private set; }
    public decimal Nota { get; private set; }
    public decimal Relevancia { get; private set; }
    public DateTime DataSubmissao { get; private set; }

    // Construtor sem parâmetros para serialização/MongoDB
    protected Trabalho() { }

    public Trabalho(
        string titulo,
        string descricao,
        string categoria,
        string linkRepositorio,
        string linkPitch,
        Guid grupoId,
        decimal nota = 0,
        decimal relevancia = 0,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        DataSubmissao = DateTime.UtcNow;
        SetGrupo(grupoId);
        Update(titulo, descricao, categoria, linkRepositorio, linkPitch, nota, relevancia);
    }

    public void SetGrupo(Guid grupoId)
    {
        if (grupoId == Guid.Empty)
            throw new DomainValidationException("O identificador do grupo é obrigatório.");

        GrupoId = grupoId;
    }

    public void AssociarGrupo(Grupo grupo)
    {
        ArgumentNullException.ThrowIfNull(grupo);
        Grupo = grupo;
        GrupoId = grupo.Id;
    }

    public void Update(
        string titulo,
        string descricao,
        string categoria,
        string linkRepositorio,
        string linkPitch,
        decimal nota,
        decimal relevancia)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length < 3)
            throw new DomainValidationException("O título do trabalho é obrigatório e deve ter ao menos 3 caracteres.");

        if (string.IsNullOrWhiteSpace(descricao) || descricao.Trim().Length < 10)
            throw new DomainValidationException("A descrição do trabalho é obrigatória e deve ter ao menos 10 caracteres.");

        if (string.IsNullOrWhiteSpace(categoria))
            throw new DomainValidationException("A categoria/challenge do trabalho é obrigatória.");

        ValidarUrl(linkRepositorio, "Link do repositório");
        ValidarUrl(linkPitch, "Link do pitch");

        AtribuirNota(nota);
        DefinirRelevancia(relevancia);

        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        Categoria = categoria.Trim();
        LinkRepositorio = linkRepositorio.Trim();
        LinkPitch = linkPitch.Trim();
    }

    public void AtribuirNota(decimal nota)
    {
        if (nota < 0 || nota > 10)
            throw new DomainValidationException("A nota do trabalho deve estar entre 0.0 e 10.0.");

        Nota = Math.Round(nota, 2);
    }

    public void DefinirRelevancia(decimal relevancia)
    {
        if (relevancia < 0 || relevancia > 100)
            throw new DomainValidationException("A relevância deve estar no intervalo de 0 a 100.");

        Relevancia = Math.Round(relevancia, 2);
    }

    private static void ValidarUrl(string url, string campo)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainValidationException($"{campo} é obrigatório.");

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainValidationException($"{campo} possui formato inválido. Deve ser uma URL HTTP/HTTPS válida.");
        }
    }
}
