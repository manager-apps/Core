using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Server.Api.Common.Result;
using Xunit;

namespace Server.Api.Tests.Common;

public class ResultExtensionsTests
{
  [Fact]
  public void ToApiResult_ReturnsOk_WhenSuccess()
  {
    var result = Result<string>.Success("data");

    var apiResult = result.ToApiResult();

    Assert.IsType<Ok<string>>(apiResult);
  }

  [Fact]
  public void ToApiResult_ReturnsCreated_WhenSuccessWithUri()
  {
    var result = Result<string>.Success("data");

    var apiResult = result.ToApiResult("/api/resource/1");

    Assert.IsType<Created<string>>(apiResult);
  }

  [Fact]
  public void ToApiResult_ReturnsNotFound_WhenNotFoundError()
  {
    var result = Result<string>.Failure(Error.NotFound("missing"));

    var apiResult = result.ToApiResult();

    Assert.IsType<NotFound<ProblemResponse>>(apiResult);
  }

  [Fact]
  public void ToApiResult_ReturnsConflict_WhenConflictError()
  {
    var result = Result<string>.Failure(Error.Conflict("already exists"));

    var apiResult = result.ToApiResult();

    Assert.IsType<Conflict<ProblemResponse>>(apiResult);
  }

  [Fact]
  public void ToApiResult_ReturnsBadRequest_WhenValidationError()
  {
    var result = Result<string>.Failure(Error.Validation("bad input"));

    var apiResult = result.ToApiResult();

    Assert.IsType<BadRequest<ProblemResponse>>(apiResult);
  }

  [Fact]
  public void ToApiResult_ReturnsForbid_WhenForbiddenError()
  {
    var result = Result<string>.Failure(Error.Forbidden("not allowed"));

    var apiResult = result.ToApiResult();

    Assert.IsType<ForbidHttpResult>(apiResult);
  }

  [Fact]
  public void ToApiResult_ReturnsUnauthorized_WhenUnauthorizedError()
  {
    var result = Result<string>.Failure(Error.Unauthorized("not authenticated"));

    var apiResult = result.ToApiResult();

    Assert.IsType<UnauthorizedHttpResult>(apiResult);
  }

  [Fact]
  public void ToApiResult_ReturnsProblem_WhenInternalError()
  {
    var result = Result<string>.Failure(Error.Internal("crash"));

    var apiResult = result.ToApiResult();

    Assert.IsType<ProblemHttpResult>(apiResult);
  }

  [Fact]
  public void ToApiResult_NotFoundResponse_ContainsCorrectCode()
  {
    var result = Result<string>.Failure(Error.NotFound("gone"));

    var apiResult = result.ToApiResult();

    var typed = Assert.IsType<NotFound<ProblemResponse>>(apiResult);
    Assert.Equal("NotFound", typed.Value!.Code);
    Assert.Equal("gone", typed.Value.Message);
  }

  [Fact]
  public void ToApiResult_ConflictResponse_ContainsCorrectCode()
  {
    var result = Result<string>.Failure(Error.Conflict("dup"));

    var apiResult = result.ToApiResult();

    var typed = Assert.IsType<Conflict<ProblemResponse>>(apiResult);
    Assert.Equal("Conflict", typed.Value!.Code);
  }
}
