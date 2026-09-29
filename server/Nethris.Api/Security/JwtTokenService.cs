using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Nethris.Api.Data;
using Nethris.Api.Domain;

namespace Nethris.Api.Security;

public sealed class JwtTokenService(HrisDatabase database)
{
    public const string Issuer = "Nethris.Learning";
    public const string Audience = "Nethris.LocalClient";
    public TokenValidationParameters ValidationParameters => new()
    {
        ValidateIssuer = true, ValidIssuer = Issuer,
        ValidateAudience = true, ValidAudience = Audience,
        ValidateLifetime = true, RequireExpirationTime = true, ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true, RequireSignedTokens = true,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromHexString(database.SigningKey)),
        NameClaimType = ClaimTypes.NameIdentifier, RoleClaimType = ClaimTypes.Role
    };
    public string Create(UserAccount user, Guid sessionId, DateTimeOffset expiresAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, sessionId.ToString())
        };
        // Role and employee identity are deliberately loaded from current database state.
        var token = new JwtSecurityToken(Issuer, Audience, claims, DateTime.UtcNow, expiresAt.UtcDateTime,
            new SigningCredentials(ValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
