namespace Shop.Contracts.User;

public sealed class UserResponse
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public required string Role { get; init; }
}
