using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;
using FiapNext.Application.Services;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Exceptions;
using FiapNext.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace FiapNext.Tests.Unit.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _sut = new AuthService(_userRepoMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarTokenJWT()
    {
        // Arrange
        var user = new User("Admin FIAP", "admin@fiap.com.br", "Admin@123", "Admin");
        var request = new LoginRequest("admin@fiap.com.br", "Admin@123");
        var tokenEsperado = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.dummy";

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(t => t.GenerateToken(user))
            .Returns(tokenEsperado);

        // Act
        var response = await _sut.LoginAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().Be(tokenEsperado);
        response.Nome.Should().Be("Admin FIAP");
        response.Email.Should().Be("admin@fiap.com.br");
        response.Role.Should().Be("Admin");
        response.ExpiraEm.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ComSenhaInvalida_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        var user = new User("Aluno FIAP", "aluno@fiap.com.br", "SenhaCerta@123", "Usuario");
        var request = new LoginRequest("aluno@fiap.com.br", "SenhaErrada");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Credenciais inválidas*");

        _tokenServiceMock.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("", "123456")]
    [InlineData("email@teste.com", "")]
    [InlineData("   ", "   ")]
    public async Task LoginAsync_ComCamposVazios_DeveLancarDomainValidationException(string email, string senha)
    {
        // Arrange
        var request = new LoginRequest(email, senha);

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*Email e senha são obrigatórios*");
    }
}
