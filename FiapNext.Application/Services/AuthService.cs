using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Exceptions;
using FiapNext.Domain.Interfaces;

namespace FiapNext.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            throw new DomainValidationException("Email e senha são obrigatórios.");

        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user == null || user.Senha != request.Senha)
            throw new UnauthorizedAccessException("Credenciais inválidas. Verifique o e-mail e a senha informados.");

        var token = _tokenService.GenerateToken(user);
        var expiraEm = DateTime.UtcNow.AddHours(4);

        return new LoginResponse(
            token,
            user.Nome,
            user.Email,
            user.Role,
            expiraEm
        );
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _userRepository.GetByEmailAsync(email, ct);
    }
}
