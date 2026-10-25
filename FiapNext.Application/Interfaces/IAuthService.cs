using FiapNext.Application.DTOs;
using FiapNext.Domain.Entities;

namespace FiapNext.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}
