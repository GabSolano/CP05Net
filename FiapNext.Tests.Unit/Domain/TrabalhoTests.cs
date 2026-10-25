using System.Globalization;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FiapNext.Tests.Unit.Domain;

public class TrabalhoTests
{
    [Fact]
    public void CriarTrabalho_ComDadosValidos_DeveInstanciarComSucesso()
    {
        // Arrange
        var titulo = "Smart Health Tracker";
        var descricao = "Sistema inteligente para monitoramento de sinais vitais";
        var categoria = "HealthTech";
        var linkRepo = "https://github.com/fiap/smart-health";
        var linkPitch = "https://youtube.com/watch?v=pitch123";
        var grupoId = Guid.NewGuid();
        var nota = 9.5m;
        var relevancia = 85.0m;

        // Act
        var trabalho = new Trabalho(titulo, descricao, categoria, linkRepo, linkPitch, grupoId, nota, relevancia);

        // Assert
        trabalho.Id.Should().NotBeEmpty();
        trabalho.Titulo.Should().Be(titulo);
        trabalho.Descricao.Should().Be(descricao);
        trabalho.Categoria.Should().Be(categoria);
        trabalho.LinkRepositorio.Should().Be(linkRepo);
        trabalho.LinkPitch.Should().Be(linkPitch);
        trabalho.GrupoId.Should().Be(grupoId);
        trabalho.Nota.Should().Be(nota);
        trabalho.Relevancia.Should().Be(relevancia);
        trabalho.DataSubmissao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    public void CriarTrabalho_ComTituloInvalido_DeveLancarDomainValidationException(string? tituloInvalido)
    {
        // Arrange
        var descricao = "Descrição válida com mais de 10 caracteres";
        var linkRepo = "https://github.com/fiap/teste";
        var linkPitch = "https://youtube.com/pitch";
        var grupoId = Guid.NewGuid();

        // Act
        Action act = () => new Trabalho(tituloInvalido!, descricao, "IA", linkRepo, linkPitch, grupoId);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*título*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Curto")]
    [InlineData("123456789")]
    public void CriarTrabalho_ComDescricaoInvalida_DeveLancarDomainValidationException(string? descricaoInvalida)
    {
        // Arrange
        var titulo = "Projeto Válido";
        var linkRepo = "https://github.com/fiap/teste";
        var linkPitch = "https://youtube.com/pitch";
        var grupoId = Guid.NewGuid();

        // Act
        Action act = () => new Trabalho(titulo, descricaoInvalida!, "IA", linkRepo, linkPitch, grupoId);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*descrição*");
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("-5")]
    [InlineData("10.1")]
    [InlineData("100")]
    public void AtribuirNota_ComValorForaDaFaixa_DeveLancarDomainValidationException(string notaString)
    {
        // Arrange
        var nota = decimal.Parse(notaString, CultureInfo.InvariantCulture);
        var trabalho = new Trabalho(
            "Projeto Teste",
            "Descrição detalhada do projeto para o festival",
            "IA",
            "https://github.com/fiap/teste",
            "https://youtube.com/pitch",
            Guid.NewGuid(),
            nota: 8.0m
        );

        // Act
        Action act = () => trabalho.AtribuirNota(nota);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*nota*deve estar entre 0.0 e 10.0*");
    }

    [Theory]
    [InlineData("0.0")]
    [InlineData("5.75")]
    [InlineData("10.0")]
    public void AtribuirNota_ComValorValido_DeveAtualizarNota(string notaString)
    {
        // Arrange
        var nota = decimal.Parse(notaString, CultureInfo.InvariantCulture);
        var trabalho = new Trabalho(
            "Projeto Teste",
            "Descrição detalhada do projeto para o festival",
            "IA",
            "https://github.com/fiap/teste",
            "https://youtube.com/pitch",
            Guid.NewGuid()
        );

        // Act
        trabalho.AtribuirNota(nota);

        // Assert
        trabalho.Nota.Should().Be(nota);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("100.1")]
    [InlineData("200")]
    public void DefinirRelevancia_ComValorInvalido_DeveLancarDomainValidationException(string relevanciaString)
    {
        // Arrange
        var relevancia = decimal.Parse(relevanciaString, CultureInfo.InvariantCulture);
        var trabalho = new Trabalho(
            "Projeto Teste",
            "Descrição detalhada do projeto para o festival",
            "IA",
            "https://github.com/fiap/teste",
            "https://youtube.com/pitch",
            Guid.NewGuid()
        );

        // Act
        Action act = () => trabalho.DefinirRelevancia(relevancia);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*relevância deve estar no intervalo de 0 a 100*");
    }

    [Theory]
    [InlineData("nao-e-url")]
    [InlineData("ftp://servidor/arquivo")]
    [InlineData("javascript:alert(1)")]
    public void CriarTrabalho_ComUrlInvalida_DeveLancarDomainValidationException(string urlInvalida)
    {
        // Arrange
        var titulo = "Projeto com Url Invalida";
        var descricao = "Descrição detalhada do projeto de inovação";
        var grupoId = Guid.NewGuid();

        // Act
        Action act = () => new Trabalho(titulo, descricao, "IA", urlInvalida, "https://youtube.com/pitch", grupoId);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*URL HTTP/HTTPS válida*");
    }
}
