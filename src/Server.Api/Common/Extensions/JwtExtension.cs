using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Server.Api.Common.Extensions;

public static class JwtExtension
{
  public static void AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
  {
    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:Jwt:Key"]!)),
          ValidateIssuer = true,
          ValidIssuer = configuration["Authentication:Jwt:Issuer"],
          ValidateAudience = true,
          ValidAudience = configuration["Authentication:Jwt:Audience"],
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero
        };
      });

    services.AddAuthorization();
  }
}
