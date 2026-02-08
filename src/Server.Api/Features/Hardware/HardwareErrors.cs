using Server.Api.Common.Result;

namespace Server.Api.Features.Hardware;

public class HardwareErrors
{
  public static Error NotFound()
    => Error.NotFound("Hardware information not found");
}
