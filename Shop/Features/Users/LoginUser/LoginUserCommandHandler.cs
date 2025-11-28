using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Abstractions.Security;
using Shop.Infrastructure;

namespace Shop.Features.Users.LoginUser;

internal sealed class LoginUserCommandHandler(
    ApplicationContext context,
    ITokenProvider tokenProvider,
    IPasswordHasher passwordHasher,
    ILogger<LoginUserCommandHandler> logger) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Login user {login}", request.Login);

        var user = await context
            .Users
            .FirstOrDefaultAsync(u => u.Login == request.Login, cancellationToken);

        if (user is null)
        {
            logger.LogError("User with login {login} not found", request.Login);
            return Result<string>.Unauthorized("Invalid credentials");
        }

        bool validPassword = passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!validPassword)
        {
            logger.LogError("Password does not match");
            return Result<string>.Unauthorized("Invalid credentials");
        }

        var token = tokenProvider.GetAccessToken(user.Id, user.Username, user.Role.ToString());

        logger.LogInformation("User successfully logged in");
        return Result<string>.Success(token);
    }
}
