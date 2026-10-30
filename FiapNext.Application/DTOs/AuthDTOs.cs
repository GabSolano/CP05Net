namespace FiapNext.Application.DTOs;

public record LoginRequest(string Email, string Senha);

public record LoginResponse(
    string Token,
    string Nome,
    string Email,
    string Role,
    DateTime ExpiraEm
);

public record RegisterUserRequest(
    string Nome,
    string Email,
    string Senha,
    string? Role
);

public record UserResponse(
    Guid Id,
    string Nome,
    string Email,
    string Role
);
