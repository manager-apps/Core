using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Server.Api.Common.Extensions;

public static class ApiVersioningExtension
{
  public static readonly ApiVersion V1 = new(1, 0);
  public static readonly ApiVersion V2 = new(2, 0);

  extension(IServiceCollection services)
  {
    public void AddApiVersioning(IConfiguration configuration)
    {
      services.AddApiVersioning(options =>
      {
        options.DefaultApiVersion = V1;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
          new UrlSegmentApiVersionReader());
      })
      .AddApiExplorer(options =>
      {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
      });
    }
  }

  extension(WebApplication app)
  {
    public ApiVersionSet CreateVersionSet()
    {
      return app.NewApiVersionSet()
        .HasApiVersion(V1)
        .HasApiVersion(V2)
        .ReportApiVersions()
        .Build();
    }
  }
}
