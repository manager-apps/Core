namespace Server.Ingest.Common.Result;

/// <summary>
/// Represents an error with a code, description, and optional validation errors.
/// </summary>
public sealed record Error(
    string Code,
    string Description,
    IReadOnlyList<ValidationError>? ValidationErrors = null)
{
    public static Error Validation(string description) => new("Validation", description);
    public static Error NotFound(string description) => new("NotFound", description);
    public static Error Conflict(string description) => new("Conflict", description);
    public static Error Forbidden(string description) => new("Forbidden", description);
    public static Error Internal(string description) => new("Internal", description);
    public static Error Unauthorized(string description) => new("Unauthorized", description);
}







