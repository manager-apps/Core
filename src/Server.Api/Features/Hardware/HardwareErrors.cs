using Server.Api.Common.Result;

namespace Server.Api.Features.Hardware;

internal static class HardwareErrors
{
  internal static Error NotFound()
    => Error.NotFound("Hardware not found.");
}
