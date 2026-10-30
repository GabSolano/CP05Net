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

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Trim().Length < 2)
            throw new DomainValidationException("O nome do usuário é obrigatório e deve conter ao menos 2 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            throw new DomainValidationException("E-mail informado é inválido.");

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            throw new DomainValidationException("A senha deve conter ao menos 6 caracteres.");

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
            throw new DomainValidationException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");

        var role = string.IsNullOrWhiteSpace(request.Role) ? "Usuario" : request.Role.Trim();
        var newUser = new User(request.Nome.Trim(), request.Email.Trim().ToLowerInvariant(), request.Senha, role);

        await _userRepository.CreateAsync(newUser, ct);

        return new UserResponse(newUser.Id, newUser.Nome, newUser.Email, newUser.Role);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(u => new UserResponse(u.Id, u.Nome, u.Email, u.Role)).ToList();
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _userRepository.GetByEmailAsync(email, ct);
    }
}
