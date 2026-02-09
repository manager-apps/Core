using Server.Api.Common.Result;

namespace Server.Api.Features.Hardware;

internal class HardwareErrors
{
  internal static Error NotFound()
    => Error.NotFound("Hardware information not found");
}
