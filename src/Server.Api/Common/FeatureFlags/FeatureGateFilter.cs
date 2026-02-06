using Microsoft.FeatureManagement;

namespace Server.Api.Common.FeatureFlags;

public class FeatureGateFilter(string featureName) : IEndpointFilter
{
  public async ValueTask<object?> InvokeAsync(
    EndpointFilterInvocationContext context,
    EndpointFilterDelegate next)
  {
    var featureManager = context.HttpContext.RequestServices
      .GetRequiredService<IFeatureManager>();
    if (!await featureManager.IsEnabledAsync(featureName))
    {
      return Results.NotFound(new
      {
        error = "Feature not available",
        feature = featureName
      });
    }

    return await next(context);
  }
}

public static class FeatureGateEndpointExtensions
{
  public static TBuilder WithFeatureGate<TBuilder>(
    this TBuilder builder,
    string featureName) where TBuilder : IEndpointConventionBuilder
  {
    return builder.AddEndpointFilter(new FeatureGateFilter(featureName));
  }
}
