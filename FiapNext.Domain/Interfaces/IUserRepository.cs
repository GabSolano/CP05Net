using FiapNext.Domain.Entities;

namespace FiapNext.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
}
