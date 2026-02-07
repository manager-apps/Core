using Microsoft.FeatureManagement;

namespace Server.Api.Common.Extensions;

public static class FeatureFlagsExtension
{
  extension(IServiceCollection services)
  {
    public void AddFeatureFlags(IConfiguration configuration)
    {
      services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
    }
  }
}
