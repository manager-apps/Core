using Server.Api.Common.Result;
using Xunit;

namespace Server.Api.Tests.Common;

public class ErrorTests
{
  [Fact]
  public void Validation_SetsCodeToValidation()
  {
    var error = Error.Validation("bad input");

    Assert.Equal("Validation", error.Code);
    Assert.Equal("bad input", error.Description);
  }

  [Fact]
  public void NotFound_SetsCodeToNotFound()
  {
    var error = Error.NotFound("not found");

    Assert.Equal("NotFound", error.Code);
    Assert.Equal("not found", error.Description);
  }

  [Fact]
  public void Conflict_SetsCodeToConflict()
  {
    var error = Error.Conflict("already exists");

    Assert.Equal("Conflict", error.Code);
    Assert.Equal("already exists", error.Description);
  }

  [Fact]
  public void Forbidden_SetsCodeToForbidden()
  {
    var error = Error.Forbidden("access denied");

    Assert.Equal("Forbidden", error.Code);
    Assert.Equal("access denied", error.Description);
  }

  [Fact]
  public void Internal_SetsCodeToInternal()
  {
    var error = Error.Internal("server error");

    Assert.Equal("Internal", error.Code);
    Assert.Equal("server error", error.Description);
  }

  [Fact]
  public void Unauthorized_SetsCodeToUnauthorized()
  {
    var error = Error.Unauthorized("not authenticated");

    Assert.Equal("Unauthorized", error.Code);
    Assert.Equal("not authenticated", error.Description);
  }

  [Fact]
  public void Error_DefaultValidationErrors_IsNull()
  {
    var error = Error.NotFound("missing");

    Assert.Null(error.ValidationErrors);
  }
}
