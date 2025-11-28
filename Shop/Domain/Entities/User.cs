using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; }

    public string Username { get; set; } = string.Empty;

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; }
}
