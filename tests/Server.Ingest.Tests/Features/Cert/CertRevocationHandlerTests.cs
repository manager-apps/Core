using Microsoft.Extensions.Logging.Abstractions;
using Server.Domain;
using Server.Ingest.Features.Cert.Revocation;
using Server.Ingest.Tests.Helpers;
using Xunit;

namespace Server.Ingest.Tests.Features.Cert;

public class CertRevocationHandlerTests
{
  private static Certificate CreateCert(long agentId, string thumbprint)
    => Certificate.Create(agentId, "serial-1", thumbprint, "CN=TestAgent",
      DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

  [Fact]
  public async Task CheckRevocationAsync_ReturnsCertificateNotFound_WhenThumbprintDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new CertRevocationHandler(NullLogger<CertRevocationHandler>.Instance, db);

    var result = await handler.CheckRevocationAsync("nonexistent-thumbprint", CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task CheckRevocationAsync_ReturnsNotRevoked_WhenCertificateIsActive()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    db.Certificates.Add(CreateCert(agent.Id, "thumbprint-abc"));
    await db.SaveChangesAsync();

    var handler = new CertRevocationHandler(NullLogger<CertRevocationHandler>.Instance, db);

    var result = await handler.CheckRevocationAsync("thumbprint-abc", CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(result.Value.IsRevoked);
    Assert.Null(result.Value.RevokedAt);
    Assert.Null(result.Value.Reason);
  }

  [Fact]
  public async Task CheckRevocationAsync_ReturnsRevoked_WithReasonAndTimestamp()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var cert = CreateCert(agent.Id, "thumbprint-xyz");
    cert.Revoke("Compromised key");
    db.Certificates.Add(cert);
    await db.SaveChangesAsync();

    var handler = new CertRevocationHandler(NullLogger<CertRevocationHandler>.Instance, db);

    var result = await handler.CheckRevocationAsync("thumbprint-xyz", CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.IsRevoked);
    Assert.NotNull(result.Value.RevokedAt);
    Assert.Equal("Compromised key", result.Value.Reason);
  }
}
