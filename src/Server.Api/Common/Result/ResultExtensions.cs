using FluentValidation.Results;

namespace Server.Api.Common.Result;

public sealed record ProblemResponse(
  string Code,
  string Message,
  IReadOnlyList<ValidationError>? Errors = null);

public sealed record ValidationError(
  string Field,
  string Message);

public static class ResultExtensions
{
  public static IResult ToApiResult<T>(
    this Result<T> result,
    string? createdUri = null)
  {
    return result.Match(
      onSuccess: value => createdUri is not null
        ? Results.Created(createdUri, value)
        : Results.Ok(value),
      onFailure: error => error.Code switch
      {
        "NotFound" => Results.NotFound(new ProblemResponse(
          error.Code, error.Description)),
        "Conflict" => Results.Conflict(new ProblemResponse(
          error.Code, error.Description)),
        "Validation" => Results.BadRequest(error.ToProblemResponse()),
        "Forbidden" => Results.Forbid(),
        "Unauthorized" => Results.Unauthorized(),
        _ => Results.Problem(error.Description)
      });
  }

  private static ProblemResponse ToProblemResponse(this Error error) =>
    new(error.Code, error.Description, error.ValidationErrors);
}

