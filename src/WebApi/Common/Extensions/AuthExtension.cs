using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebApi.Common.Interfaces;
using WebApi.Common.Options;
using WebApi.Infrastructure;

namespace WebApi.Common.Extensions;

public static class AuthExtension
{

  extension(IServiceCollection services)
  {
    public void AddAuth(IConfiguration configuration)
    {
      var jwtSection = configuration.GetSection("Jwt")
          ?? throw new InvalidOperationException("JWT configuration section is missing.");

      services.Configure<JwtOption>(jwtSection);
      services.AddSingleton<IDataHasher, HmacDataHasher>();
      services.AddSingleton<IJwtProvider, JwtProvider>();

      services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
          var jwtSettings = jwtSection.Get<JwtOption>()!;
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
          };
        });

      services.AddAuthorization();
    }
  }
}
