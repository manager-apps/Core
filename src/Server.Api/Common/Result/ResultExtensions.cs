using FluentValidation.Results;

namespace Server.Api.Common.Result;

/// <summary>
/// Standard error response for API errors.
/// </summary>
public sealed record ProblemResponse(
    string Code,
    string Message,
    IReadOnlyList<ValidationError>? Errors = null);

/// <summary>
/// Represents a single validation error.
/// </summary>
public sealed record ValidationError(
    string Field,
    string Message);

public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, string? createdUri = null)
    {
        return result.Match(
            onSuccess: value => createdUri is not null
                ? Results.Created(createdUri, value)
                : Results.Ok(value),
            onFailure: error => error.Code switch
            {
                "NotFound" => Results.NotFound(new ProblemResponse(error.Code, error.Description)),
                "Conflict" => Results.Conflict(new ProblemResponse(error.Code, error.Description)),
                "Validation" => Results.BadRequest(error.ToProblemResponse()),
                "Forbidden" => Results.Forbid(),
                "Unauthorized" => Results.Unauthorized(),
                _ => Results.Problem(error.Description)
            });
    }

    public static Error ToError(this ValidationResult validationResult)
    {
        var validationErrors = validationResult.Errors
            .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
            .ToList();

        return new Error("Validation", "Validation errors", validationErrors);
    }

    private static ProblemResponse ToProblemResponse(this Error error)
    {
        return new ProblemResponse(error.Code, error.Description, error.ValidationErrors);
    }
}
