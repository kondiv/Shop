namespace Shop.Contracts.User;

public sealed class UserRegistrationRequest
{
    public required string Login { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string Role { get; init; }
}
