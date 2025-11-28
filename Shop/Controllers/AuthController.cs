using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shop.Contracts.User;
using Shop.Features.Users.LoginUser;
using Shop.Features.Users.RegisterUser;
using Shop.Infrastructure.Security.Tokens;

namespace Shop.Controllers;

[ApiController]
[Route("api/auth/")]
public sealed class AuthController(IMediator mediator, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private readonly int _tokenLifetime = jwtOptions.Value.LifetimeSec;

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> RegisterAsync(
        UserRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterUserCommand(request.Login, request.Username, request.Password, request.Role);

        var registrationResult = await mediator.Send(command, cancellationToken);

        return registrationResult.ToActionResult(this);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> LoginAsync(
        UserLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginUserCommand(request.Login, request.Password);

        var loginResult = await mediator.Send(command, cancellationToken);

        if (loginResult.IsSuccess)
        {
            HttpContext.Response.Cookies.Append("access_token", loginResult.Value, new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddSeconds(_tokenLifetime)
            });
        }

        return loginResult.ToActionResult(this);
    }
}
