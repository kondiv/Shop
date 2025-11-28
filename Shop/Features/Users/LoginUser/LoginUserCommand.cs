using Ardalis.Result;
using MediatR;

namespace Shop.Features.Users.LoginUser;

internal sealed record LoginUserCommand(string Login, string Password) : IRequest<Result<string>>;
