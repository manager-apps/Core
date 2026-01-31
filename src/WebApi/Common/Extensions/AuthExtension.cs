using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebApi.Common.Interfaces;
using WebApi.Common.Options;
using WebApi.Infrastructure;

namespace WebApi.Common.Extensions;

public static class AuthExtension
{
  public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<IDataHasher, HmacDataHasher>();
    services.AddSingleton<IJwtProvider, JwtProvider>();

    services.Configure<JwtOption>(configuration.GetSection("Jwt"));

    services.AddAuthentication("Bearer")
      .AddJwtBearer("Bearer", options =>
      {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtOption>();
        if (jwtSettings is null)
          throw new InvalidOperationException("JWT settings are not configured properly.");

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
    return services;
  }
}
