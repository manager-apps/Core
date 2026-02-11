using Microsoft.Extensions.Caching.Hybrid;
using Server.Api.Common.Options;

namespace Server.Api.Common.Extensions;

public static class HybridCacheExtension
{
  extension(IServiceCollection services)
  {
    public void AddHybridCache(IConfiguration configuration)
    {
      var redisOption = configuration
        .GetSection(RedisOption.SectionName)
        .Get<RedisOption>()!;

      services.AddStackExchangeRedisCache(
        options =>
        {
          options.Configuration = redisOption.ConnectionString;
          options.InstanceName = redisOption.InstanceName;
        });

      services.AddHybridCache(options =>
      {
        options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
          Expiration = TimeSpan.FromMinutes(5),
          LocalCacheExpiration = TimeSpan.FromMinutes(5)
        };
      });
    }
  }
}
