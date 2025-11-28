namespace Shop.Abstractions.Security;

public interface ITokenProvider
{
    string GetAccessToken(Guid userId, string username, string role);
}
