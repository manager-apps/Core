using Server.Api.Common.Result;

namespace Server.Api.Features.Config;

internal static class ConfigErrors
{
  internal static Error NotFound()
    => Error.NotFound("Config not found");
}
