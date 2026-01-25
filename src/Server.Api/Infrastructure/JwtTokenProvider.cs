using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Api.Common.Interfaces;
using Server.Api.Common.Options;

namespace Server.Api.Infrastructure;

public class JwtTokenProvider(IOptions<JwtOption> settings) : IJwtTokenProvider
{
  private readonly JwtOption _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

  public string GenerateTokenForUser(string username, string role)
    => GenerateToken(new Dictionary<string, string>
    {
      { ClaimTypes.Name, username },
      { ClaimTypes.Role, role },
    });

  public string GenerateTokenForAgent(string agentName)
    => GenerateToken(new Dictionary<string, string>
    {
      { ClaimTypes.Name, agentName },
      { ClaimTypes.Role, "Agent" },
    });

  private string GenerateToken(Dictionary<string, string> claims)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var jwtClaims = claims.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList();
    jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

    var token = new JwtSecurityToken(
      issuer: _settings.Issuer,
      audience: _settings.Audience,
      claims: jwtClaims,
      expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
