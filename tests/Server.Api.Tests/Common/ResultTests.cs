using Server.Api.Common.Result;
using Xunit;

namespace Server.Api.Tests.Common;

public class ResultTests
{
  [Fact]
  public void Success_IsSuccess_IsTrue()
  {
    var result = Result<string>.Success("value");

    Assert.True(result.IsSuccess);
    Assert.False(result.IsFailure);
  }

  [Fact]
  public void Failure_IsFailure_IsTrue()
  {
    var result = Result<string>.Failure(Error.NotFound("missing"));

    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailure);
  }

  [Fact]
  public void Success_Value_ReturnsValue()
  {
    var result = Result<int>.Success(42);

    Assert.Equal(42, result.Value);
  }

  [Fact]
  public void Failure_Error_ReturnsError()
  {
    var error = Error.Conflict("dup");
    var result = Result<int>.Failure(error);

    Assert.Equal(error, result.Error);
  }

  [Fact]
  public void Success_AccessingError_Throws()
  {
    var result = Result<string>.Success("ok");

    Assert.Throws<InvalidOperationException>(() => result.Error);
  }

  [Fact]
  public void Failure_AccessingValue_Throws()
  {
    var result = Result<string>.Failure(Error.Internal("oops"));

    Assert.Throws<InvalidOperationException>(() => result.Value);
  }

  [Fact]
  public void Match_CallsOnSuccess_WhenSuccessful()
  {
    var result = Result<string>.Success("hello");

    var output = result.Match(v => v.ToUpper(), _ => "failed");

    Assert.Equal("HELLO", output);
  }

  [Fact]
  public void Match_CallsOnFailure_WhenFailed()
  {
    var result = Result<string>.Failure(Error.NotFound("gone"));

    var output = result.Match(_ => "ok", e => e.Code);

    Assert.Equal("NotFound", output);
  }

  [Fact]
  public void ImplicitConversion_FromValue_CreatesSuccess()
  {
    Result<string> result = "implicit";

    Assert.True(result.IsSuccess);
    Assert.Equal("implicit", result.Value);
  }

  [Fact]
  public void ImplicitConversion_FromError_CreatesFailure()
  {
    Result<string> result = Error.Forbidden("no");

    Assert.True(result.IsFailure);
    Assert.Equal("Forbidden", result.Error.Code);
  }

  [Fact]
  public void NonGenericResult_Success_IsSuccess()
  {
    var result = Result.Success();

    Assert.True(result.IsSuccess);
    Assert.False(result.IsFailure);
  }

  [Fact]
  public void NonGenericResult_Failure_IsFailure()
  {
    var result = Result.Failure(Error.Internal("fail"));

    Assert.False(result.IsSuccess);
    Assert.Equal("Internal", result.Error.Code);
  }
}
