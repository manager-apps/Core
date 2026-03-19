using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Auth.Login.Google;

internal interface IGoogleLoginHandler
{
  Task<AuthTokenResponse?> HandleAsync(GoogleLoginRequest request, CancellationToken ct);
}

internal class GoogleLoginHandler(
  ILogger<GoogleLoginHandler> logger,
  AppDbContext dbContext,
  IConfiguration configuration,
  IHttpClientFactory httpClientFactory) : IGoogleLoginHandler
{
  public async Task<AuthTokenResponse?> HandleAsync(GoogleLoginRequest request, CancellationToken ct)
  {
    var client = httpClientFactory.CreateClient();
    var response = await client.GetAsync(
      $"https://oauth2.googleapis.com/tokeninfo?id_token={request.Credential}", ct);

    if (!response.IsSuccessStatusCode)
    {
      logger.LogWarning("Google token validation failed with status {Status}", response.StatusCode);
      return null;
    }

    var tokenInfo = await response.Content.ReadFromJsonAsync<GoogleTokenInfo>(ct);
    if (tokenInfo is null)
    {
      logger.LogWarning("Failed to parse Google token info response");
      return null;
    }

    if (tokenInfo.Aud != configuration["Authentication:Google:ClientId"])
    {
      logger.LogWarning("Google token audience mismatch");
      return null;
    }

    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.GoogleId == tokenInfo.Sub, ct);
    if (user is null)
    {
      user = Server.Domain.User.Create(tokenInfo.Sub, tokenInfo.Email, tokenInfo.Name, tokenInfo.Picture);
      dbContext.Users.Add(user);
      await dbContext.SaveChangesAsync(ct);
    }

    return new AuthTokenResponse(GenerateJwt(user), user.Name, user.Email, user.AvatarUrl);
  }

  private string GenerateJwt(Server.Domain.User user)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:Jwt:Key"]!));
    var token = new JwtSecurityToken(
      issuer: configuration["Authentication:Jwt:Issuer"],
      audience: configuration["Authentication:Jwt:Audience"],
      claims:
      [
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Name, user.Name),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      ],
      expires: DateTime.UtcNow.AddHours(24),
      signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );
    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
