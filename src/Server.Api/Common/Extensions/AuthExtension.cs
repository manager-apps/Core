using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Server.Api.Common.Interfaces;
using Server.Api.Common.Options;
using Server.Api.Infrastructure;

namespace Server.Api.Common.Extensions;

public static class AuthExtension
{
  public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<IPasswordHasher, HmacPasswordHasher>();
    services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

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
