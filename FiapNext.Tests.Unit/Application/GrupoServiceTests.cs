using FiapNext.Application.DTOs;
using FiapNext.Application.Services;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace FiapNext.Tests.Unit.Application;

public class GrupoServiceTests
{
    private readonly Mock<IGrupoRepository> _grupoRepoMock;
    private readonly GrupoService _sut;

    public GrupoServiceTests()
    {
        _grupoRepoMock = new Mock<IGrupoRepository>();
        _sut = new GrupoService(_grupoRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DevePersistirERetornarDto()
    {
        // Arrange
        var request = new CreateGrupoRequest(
            "DevOps Stars",
            "2TDSPV",
            new List<string> { "Aluno A", "Aluno B" }
        );

        _grupoRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Grupo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be(request.Nome);
        result.Turma.Should().Be(request.Turma);
        result.Integrantes.Should().Equal(request.Integrantes);

        _grupoRepoMock.Verify(r => r.CreateAsync(It.IsAny<Grupo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_QuandoExiste_DeveRetornarGrupo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var grupo = new Grupo("Cloud Masters", "2TDSPV", new[] { "Aluno 1" }, id);

        _grupoRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Nome.Should().Be("Cloud Masters");
    }

    [Fact]
    public async Task UpdateAsync_QuandoGrupoNaoExiste_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = new UpdateGrupoRequest("Novo Nome", "2TDSPV", new List<string> { "Aluno 1" });

        _grupoRepoMock
            .Setup(r => r.GetByIdAsync(idInexistente, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Grupo?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAsync(idInexistente, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{idInexistente}*não foi encontrado*");
    }

    [Fact]
    public async Task DeleteAsync_DeveChamarRepositorio()
    {
        // Arrange
        var id = Guid.NewGuid();
        _grupoRepoMock
            .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _grupoRepoMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
