using FiapNext.Domain.Entities;
using FiapNext.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FiapNext.Tests.Unit.Domain;

public class GrupoTests
{
    [Fact]
    public void CriarGrupo_ComDadosValidos_DeveInstanciarComSucesso()
    {
        // Arrange
        var nome = "Tech Titans";
        var turma = "2TDSPV";
        var integrantes = new List<string> { "Gabriel Silva", "Mariana Souza", "Lucas Lima" };

        // Act
        var grupo = new Grupo(nome, turma, integrantes);

        // Assert
        grupo.Id.Should().NotBeEmpty();
        grupo.Nome.Should().Be(nome);
        grupo.Turma.Should().Be(turma);
        grupo.Integrantes.Should().HaveCount(3)
            .And.Contain(integrantes);
        grupo.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void CriarGrupo_ComNomeInvalido_DeveLancarDomainValidationException(string? nomeInvalido)
    {
        // Arrange
        var turma = "2TDSPV";
        var integrantes = new List<string> { "Aluno Teste" };

        // Act
        Action act = () => new Grupo(nomeInvalido!, turma, integrantes);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*nome do grupo é obrigatório*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CriarGrupo_ComTurmaInvalida_DeveLancarDomainValidationException(string? turmaInvalida)
    {
        // Arrange
        var nome = "Equipe Alpha";
        var integrantes = new List<string> { "Aluno Teste" };

        // Act
        Action act = () => new Grupo(nome, turmaInvalida!, integrantes);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*turma/classe é obrigatória*");
    }

    [Fact]
    public void CriarGrupo_SemIntegrantes_DeveLancarDomainValidationException()
    {
        // Arrange
        var nome = "Equipe Sem Alunos";
        var turma = "2TDSPV";
        var integrantes = new List<string>();

        // Act
        Action act = () => new Grupo(nome, turma, integrantes);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*pelo menos um integrante válido*");
    }
}
