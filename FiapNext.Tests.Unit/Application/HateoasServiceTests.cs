using FiapNext.Application.DTOs;
using FiapNext.Application.Services;
using FluentAssertions;
using Xunit;

namespace FiapNext.Tests.Unit.Application;

public class HateoasServiceTests
{
    private readonly HateoasService _sut = new();
    private const string BaseUrl = "https://localhost:7001";

    [Fact]
    public void EnvelopTrabalho_QuandoUsuarioEhAdmin_DeveIncluirLinksDeAlteracaoEDelete()
    {
        // Arrange
        var trabalho = new TrabalhoResponse(
            Guid.NewGuid(),
            "Next Level IA",
            "Descrição do projeto com detalhes",
            "IA",
            "https://github.com/fiap/ai",
            "https://youtube.com/pitch",
            Guid.NewGuid(),
            "Grupo Next",
            9.5m,
            90m,
            DateTime.UtcNow
        );

        // Act
        var envelope = _sut.EnvelopTrabalho(trabalho, isAdmin: true, BaseUrl);

        // Assert
        envelope.Data.Should().Be(trabalho);
        envelope.Links.Should().Contain(l => l.Rel == "self" && l.Method == "GET");
        envelope.Links.Should().Contain(l => l.Rel == "grupo" && l.Method == "GET");
        envelope.Links.Should().Contain(l => l.Rel == "collection" && l.Method == "GET");
        envelope.Links.Should().Contain(l => l.Rel == "update" && l.Method == "PUT");
        envelope.Links.Should().Contain(l => l.Rel == "delete" && l.Method == "DELETE");
    }

    [Fact]
    public void EnvelopTrabalho_QuandoUsuarioEhComum_NaoDeveIncluirLinksDeAlteracaoOuDelete()
    {
        // Arrange
        var trabalho = new TrabalhoResponse(
            Guid.NewGuid(),
            "Next Level IA",
            "Descrição do projeto com detalhes",
            "IA",
            "https://github.com/fiap/ai",
            "https://youtube.com/pitch",
            Guid.NewGuid(),
            "Grupo Next",
            9.5m,
            90m,
            DateTime.UtcNow
        );

        // Act
        var envelope = _sut.EnvelopTrabalho(trabalho, isAdmin: false, BaseUrl);

        // Assert
        envelope.Data.Should().Be(trabalho);
        envelope.Links.Should().Contain(l => l.Rel == "self");
        envelope.Links.Should().NotContain(l => l.Rel == "update");
        envelope.Links.Should().NotContain(l => l.Rel == "delete");
    }

    [Fact]
    public void EnvelopTrabalhosPaged_DeveCalcularPaginacaoEHiperlinksCorretamente()
    {
        // Arrange
        var item1 = new TrabalhoResponse(Guid.NewGuid(), "T1", "Descricao 10 caracteres", "IA", "https://g.com", "https://y.com", Guid.NewGuid(), null, 8, 80, DateTime.UtcNow);
        var item2 = new TrabalhoResponse(Guid.NewGuid(), "T2", "Descricao 10 caracteres", "IA", "https://g.com", "https://y.com", Guid.NewGuid(), null, 9, 90, DateTime.UtcNow);
        var items = new List<TrabalhoResponse> { item1, item2 };

        var filter = new TrabalhoFilterParams(Skip: 10, Limit: 10);
        long totalItems = 35; // Total 35 itens -> 4 páginas (1..10, 11..20, 21..30, 31..35)

        // Act
        var pagedResult = _sut.EnvelopTrabalhosPaged(items, filter, totalItems, isAdmin: true, BaseUrl);

        // Assert
        pagedResult.TotalItems.Should().Be(35);
        pagedResult.TotalPages.Should().Be(4);
        pagedResult.CurrentPage.Should().Be(2); // Skip 10 / Limit 10 + 1 = página 2
        pagedResult.Items.Should().HaveCount(2);

        // Deve conter links de navegação
        pagedResult.Links.Should().Contain(l => l.Rel == "self");
        pagedResult.Links.Should().Contain(l => l.Rel == "first");
        pagedResult.Links.Should().Contain(l => l.Rel == "prev");
        pagedResult.Links.Should().Contain(l => l.Rel == "next");
        pagedResult.Links.Should().Contain(l => l.Rel == "last");
        pagedResult.Links.Should().Contain(l => l.Rel == "create");
    }
}
