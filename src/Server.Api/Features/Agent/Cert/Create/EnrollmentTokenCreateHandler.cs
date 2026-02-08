using System.Security.Cryptography;
using Server.Api.Common.Interfaces;
using Server.Api.Common.Result;
using Server.Api.Features.Cert;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Cert.Create;

/// <summary>
/// Handler for creating enrollment tokens.
/// </summary>
public interface IEnrollmentTokenCreateHandler
{
  /// <summary>
  /// Creates a new enrollment token for an agent (admin only).
  /// </summary>
  Task<Result<EnrollmentTokenResponse>> HandleAsync(
    CreateEnrollmentTokenRequest request,
    CancellationToken cancellationToken);
}

internal sealed class EnrollmentTokenCreateHandler(
  ILogger<EnrollmentTokenCreateHandler> logger,
  IDataHasher dataHasher,
  AppDbContext dbContext) : IEnrollmentTokenCreateHandler
{
  public async Task<Result<EnrollmentTokenResponse>> HandleAsync(
    CreateEnrollmentTokenRequest request,
    CancellationToken cancellationToken)
  {
    var tokenBytes = RandomNumberGenerator.GetBytes(32);
    var token = Convert.ToBase64String(tokenBytes);

    var (hash, salt) = dataHasher.CreateDataHash(token);
    var validity = TimeSpan.FromHours(request.ValidityHours);
    var enrollmentToken = Domain.EnrollmentToken.Create(
      request.AgentName,
      hash,
      salt,
      validity);

    dbContext.EnrollmentTokens.Add(enrollmentToken);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Created enrollment token for agent: {AgentName}, expires: {ExpiresAt}",
      request.AgentName,
      enrollmentToken.ExpiresAt);

    return new EnrollmentTokenResponse(
      Token: token,
      AgentName: request.AgentName,
      ExpiresAt: enrollmentToken.ExpiresAt);
  }
}
