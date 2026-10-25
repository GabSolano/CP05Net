using FiapNext.Application.Common.Metrics;
using FiapNext.Application.DTOs;
using FiapNext.Application.Services;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Exceptions;
using FiapNext.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace FiapNext.Tests.Unit.Application;

public class TrabalhoServiceTests
{
    private readonly Mock<ITrabalhoRepository> _trabalhoRepoMock;
    private readonly Mock<IGrupoRepository> _grupoRepoMock;
    private readonly Mock<IFiapNextMetrics> _metricsMock;
    private readonly TrabalhoService _sut;

    public TrabalhoServiceTests()
    {
        _trabalhoRepoMock = new Mock<ITrabalhoRepository>();
        _grupoRepoMock = new Mock<IGrupoRepository>();
        _metricsMock = new Mock<IFiapNextMetrics>();
        _sut = new TrabalhoService(_trabalhoRepoMock.Object, _grupoRepoMock.Object, _metricsMock.Object);
    }

    [Fact]
    public async Task CreateAsync_QuandoGrupoNaoExiste_DeveLancarDomainValidationException()
    {
        // Arrange
        var grupoId = Guid.NewGuid();
        var request = new CreateTrabalhoRequest(
            "FIAP Next Portal",
            "Portal corporativo de gerenciamento de desafios",
            "Web",
            "https://github.com/fiap/portal",
            "https://youtube.com/pitch",
            grupoId,
            Nota: 8.5m,
            Relevancia: 90m
        );

        _grupoRepoMock
            .Setup(r => r.GetByIdAsync(grupoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Grupo?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage($"*{grupoId}*não foi encontrado*");

        _trabalhoRepoMock.Verify(r => r.CreateAsync(It.IsAny<Trabalho>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_QuandoDadosSaoValidos_DeveSalvarERegistrarMetrica()
    {
        // Arrange
        var grupoId = Guid.NewGuid();
        var grupo = new Grupo("Grupo Alpha", "2TDSPV", new[] { "Aluno 1" }, grupoId);
        var request = new CreateTrabalhoRequest(
            "FIAP Next Portal",
            "Portal corporativo de gerenciamento de desafios",
            "Web",
            "https://github.com/fiap/portal",
            "https://youtube.com/pitch",
            grupoId,
            Nota: 9.0m,
            Relevancia: 95m
        );

        _grupoRepoMock
            .Setup(r => r.GetByIdAsync(grupoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        _trabalhoRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Trabalho>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Titulo.Should().Be(request.Titulo);
        result.Categoria.Should().Be(request.Categoria);
        result.GrupoId.Should().Be(grupoId);
        result.GrupoNome.Should().Be("Grupo Alpha");

        _trabalhoRepoMock.Verify(r => r.CreateAsync(It.IsAny<Trabalho>(), It.IsAny<CancellationToken>()), Times.Once);
        _metricsMock.Verify(m => m.RecordTrabalhoSubmetido(request.Categoria), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_QuandoTrabalhoExiste_DeveRetornarDtoComGrupo()
    {
        // Arrange
        var grupoId = Guid.NewGuid();
        var grupo = new Grupo("Cyber Devs", "2TDSPV", new[] { "Aluno 1" }, grupoId);
        var trabalho = new Trabalho(
            "Cyber Security Shield",
            "Proteção e firewall inteligente para APIs corporativas",
            "Security",
            "https://github.com/fiap/shield",
            "https://youtube.com/pitch",
            grupoId,
            nota: 10m,
            relevancia: 100m
        );

        _trabalhoRepoMock
            .Setup(r => r.GetByIdAsync(trabalho.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trabalho);

        _grupoRepoMock
            .Setup(r => r.GetByIdAsync(grupoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        // Act
        var result = await _sut.GetByIdAsync(trabalho.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(trabalho.Id);
        result.Titulo.Should().Be(trabalho.Titulo);
        result.GrupoNome.Should().Be(grupo.Nome);
    }

    [Fact]
    public async Task GetPagedAsync_DeveRetornarListaPaginadaETotal()
    {
        // Arrange
        var grupoId = Guid.NewGuid();
        var grupo = new Grupo("Equipe Beta", "2TDSPV", new[] { "Aluno 1" }, grupoId);
        var trabalho = new Trabalho(
            "AI Assistant",
            "Assistente virtual para estudantes de TI",
            "IA",
            "https://github.com/fiap/ai",
            "https://youtube.com/pitch",
            grupoId,
            nota: 9m,
            relevancia: 80m
        );

        var filter = new TrabalhoFilterParams(Titulo: "AI", Skip: 0, Limit: 10);

        _trabalhoRepoMock
            .Setup(r => r.GetPagedAsync(
                filter.Titulo,
                filter.MinNota,
                filter.MaxNota,
                filter.MinRelevancia,
                filter.MaxRelevancia,
                filter.SortBy,
                true,
                filter.Skip,
                filter.Limit,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([trabalho], 1));

        _grupoRepoMock
            .Setup(r => r.GetByIdAsync(grupoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        // Act
        var (items, totalCount) = await _sut.GetPagedAsync(filter);

        // Assert
        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].Titulo.Should().Be("AI Assistant");
        items[0].GrupoNome.Should().Be("Equipe Beta");
    }

    [Fact]
    public async Task UpdateAsync_QuandoTrabalhoNaoExiste_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var updateDto = new UpdateTrabalhoRequest(
            "Novo Titulo",
            "Nova descricao detalhada com mais de 10 caracteres",
            "Mobile",
            "https://github.com/fiap/novo",
            "https://youtube.com/novo",
            Nota: 10,
            Relevancia: 90
        );

        _trabalhoRepoMock
            .Setup(r => r.GetByIdAsync(idInexistente, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trabalho?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(idInexistente, updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{idInexistente}*não foi encontrado*");
    }

    [Fact]
    public async Task DeleteAsync_DeveChamarRepositorioERetornarResultado()
    {
        // Arrange
        var id = Guid.NewGuid();
        _trabalhoRepoMock
            .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _trabalhoRepoMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
