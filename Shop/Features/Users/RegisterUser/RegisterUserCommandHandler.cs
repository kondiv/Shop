using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Abstractions.Security;
using Shop.Contracts.User;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Infrastructure;

namespace Shop.Features.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    ApplicationContext context,
    IValidator<RegisterUserCommand> validator,
    IPasswordHasher passwordHasher,
    ILogger<RegisterUserCommandHandler> logger) : IRequestHandler<RegisterUserCommand, Result<UserResponse>>
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Result<UserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering new user {login}", request.Login);

        await _lock.WaitAsync(cancellationToken);

        try
        {
            var commandValidationResult = validator.Validate(request);

            if (!commandValidationResult.IsValid)
            {
                logger.LogError("Invalid data provided. Errors\n{errors}", commandValidationResult.Errors);
                return Result<UserResponse>.Invalid(
                    commandValidationResult
                        .Errors
                        .Select(e => new ValidationError
                        {
                            ErrorCode = e.ErrorCode,
                            ErrorMessage = e.ErrorMessage,
                        })
                        .ToArray()
                );
            }

            var userExists = await context
                .Users
                .AnyAsync(u => u.Login == request.Login, cancellationToken);

            if (userExists)
            {
                logger.LogError("User with the same login {login} already exists", request.Login);
                return Result<UserResponse>.Conflict("Login already taken");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Login = request.Login,
                PasswordHash = passwordHasher.HashPassword(request.Password),
                Role = Enum.Parse<Role>(request.Role, ignoreCase: true)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("User created successfully");
            return Result<UserResponse>.Success(new UserResponse
            {
                Id = user.Id,
                Role = user.Role.ToString(),
                Username = user.Username
            });
        }
        finally
        {
            _lock.Release();
        }
    }
}
