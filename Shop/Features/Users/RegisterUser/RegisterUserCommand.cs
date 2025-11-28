using Ardalis.Result;
using MediatR;
using Shop.Contracts.User;

namespace Shop.Features.Users.RegisterUser;

internal sealed record RegisterUserCommand(
    string Login,
    string Username, 
    string Password,
    string Role)
    : IRequest<Result<UserResponse>>;
