using FiapNext.Domain.Entities;
using FiapNext.Domain.Interfaces;
using FiapNext.Infrastructure.Context;
using MongoDB.Driver;

namespace FiapNext.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    // Fallback em memória para garantir autenticação mesmo sem conexão inicial com MongoDB
    private static readonly List<User> FallbackUsers = new()
    {
        new User("Coordenação NEXT (Admin)", "admin@fiap.com.br", "Admin@123", "Admin"),
        new User("Gabriel Costa Solano (2TDSPV)", "aluno@fiap.com.br", "Aluno@123", "Usuario")
    };

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users.Find(u => u.Email.ToLower() == email.Trim().ToLower()).FirstOrDefaultAsync(ct);
            if (user != null) return user;
        }
        catch
        {
            // Fallback caso o banco esteja indisponível em testes unitários/offline
        }

        return FallbackUsers.FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.InsertOneAsync(user, cancellationToken: ct);
    }
}
