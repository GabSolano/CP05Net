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

    [Fact]
    public async Task RegisterAsync_ComDadosValidos_DevePersistirERetornarUserResponse()
    {
        // Arrange
        var request = new RegisterUserRequest("Consultor NEXT", "consultor@fiap.com.br", "SenhaForte@123", "Consultor");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be(request.Nome);
        result.Email.Should().Be(request.Email.ToLower());
        result.Role.Should().Be("Consultor");
        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ComEmailJaExistente_DeveLancarDomainValidationException()
    {
        // Arrange
        var userExistente = new User("User Já Existe", "duplicado@fiap.com.br", "123456", "Usuario");
        var request = new RegisterUserRequest("Novo", "duplicado@fiap.com.br", "123456", "Usuario");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userExistente);

        // Act
        Func<Task> act = async () => await _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*Já existe um usuário cadastrado*");
    }

    [Fact]
    public async Task GetAllUsersAsync_DeveRetornarListaDeUsuarios()
    {
        // Arrange
        var users = new List<User>
        {
            new User("Admin", "admin@fiap.com.br", "123456", "Admin"),
            new User("Consultor", "consultor@fiap.com.br", "123456", "Consultor")
        };

        _userRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Email.Should().Be("admin@fiap.com.br");
        result[1].Role.Should().Be("Consultor");
    }
}
