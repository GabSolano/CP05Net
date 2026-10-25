namespace FiapNext.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Senha { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty; // "Admin" ou "Usuario"

    protected User() { }

    public User(string nome, string email, string senha, string role, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Nome = nome;
        Email = email;
        Senha = senha;
        Role = role;
    }
}
