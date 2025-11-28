using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shop.Abstractions.Security;
using System.Security.Claims;
using System.Text;

namespace Shop.Infrastructure.Security.Tokens;

internal sealed class JwtTokenProvider(IOptions<JwtOptions> jwtOptions) : ITokenProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string GetAccessToken(Guid userId, string username, string role)
    {
        var secretKey = Encoding.UTF8.GetBytes(_jwtOptions.Key);
        var tokenHandler = new JsonWebTokenHandler();

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, userId.ToString()),
            new (ClaimTypes.Role, role),
            new ("username", username)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(_jwtOptions.LifetimeSec),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.CreateToken(tokenDescriptor);
    }
}
