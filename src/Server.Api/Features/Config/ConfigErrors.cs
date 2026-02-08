using Server.Api.Common.Result;

namespace Server.Api.Features.Config;

public static class ConfigErrors
{
  public static Error NotFound()
    => Error.NotFound("Config not found");
}
