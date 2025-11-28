namespace Shop.Contracts.User;

public sealed class UserLoginRequest
{
    public required string Login { get; init; }
    
    public required string Password { get; init; }
}
