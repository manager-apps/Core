using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.Cert.Revoke;
using Xunit;
using Server.Api.Features.Cert;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentCertRevokeHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentCertRevokeHandler(NullLogger<AgentCertRevokeHandler>.Instance, db);
    var request = new RevokeRequest("Security policy");

    var result = await handler.HandleAsync(999, request, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenNoCertificatesToRevoke()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var handler = new AgentCertRevokeHandler(NullLogger<AgentCertRevokeHandler>.Instance, db);
    var request = new RevokeRequest("No certs");

    var result = await handler.HandleAsync(agent.Id, request, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_RevokesActiveCertificates()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var cert = Certificate.Create(
      agent.Id,
      "serial001",
      "thumb001",
      "CN=TestAgent",
      DateTimeOffset.UtcNow.AddDays(-10),
      DateTimeOffset.UtcNow.AddDays(355));
    db.Certificates.Add(cert);
    await db.SaveChangesAsync();

    var handler = new AgentCertRevokeHandler(NullLogger<AgentCertRevokeHandler>.Instance, db);
    var request = new RevokeRequest("Compromised");

    var result = await handler.HandleAsync(agent.Id, request, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value);

    var revokedCert = db.Certificates.Single();
    Assert.False(revokedCert.IsActive);
    Assert.NotNull(revokedCert.RevokedAt);
    Assert.Equal("Compromised", revokedCert.RevocationReason);
  }

  [Fact]
  public async Task HandleAsync_SkipsAlreadyRevokedCertificates()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var cert = Certificate.Create(
      agent.Id,
      "serial001",
      "thumb001",
      "CN=TestAgent",
      DateTimeOffset.UtcNow.AddDays(-10),
      DateTimeOffset.UtcNow.AddDays(355));
    cert.Revoke("Already revoked");
    db.Certificates.Add(cert);
    await db.SaveChangesAsync();

    var handler = new AgentCertRevokeHandler(NullLogger<AgentCertRevokeHandler>.Instance, db);
    var request = new RevokeRequest("Trying again");

    var result = await handler.HandleAsync(agent.Id, request, CancellationToken.None);

    Assert.True(result.IsFailure); // No active certs to revoke
    Assert.Equal("NotFound", result.Error.Code);
  }
}
