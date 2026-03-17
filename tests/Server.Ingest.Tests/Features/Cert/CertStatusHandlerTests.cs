using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Server.Domain;
using Server.Ingest.Common.Options;
using Server.Ingest.Features.Cert;
using Server.Ingest.Features.Cert.Status;
using Server.Ingest.Tests.Helpers;
using Xunit;

namespace Server.Ingest.Tests.Features.Cert;

public class CertStatusHandlerTests
{
  private static ClaimsPrincipal CreatePrincipal(string? agentName)
  {
    var identity = agentName is null
      ? new ClaimsIdentity()
      : new ClaimsIdentity([new Claim(ClaimTypes.Name, agentName)], "Certificate");
    return new ClaimsPrincipal(identity);
  }

  private static IOptions<MtlsOptions> CreateOptions(int renewalThresholdDays = 30)
    => Options.Create(new MtlsOptions { RenewalThresholdDays = renewalThresholdDays });

  private static Certificate CreateActiveCert(long agentId, DateTimeOffset? expiresAt = null)
    => Certificate.Create(agentId, "serial-1", "thumb-1", "CN=PC-001",
      DateTimeOffset.UtcNow.AddDays(-10),
      expiresAt ?? DateTimeOffset.UtcNow.AddYears(1));

  [Fact]
  public async Task GetStatusAsync_ReturnsUnauthorized_WhenAgentNameIsEmpty()
  {
    using var db = DbContextFactory.Create();
    var handler = new CertStatusHandler(
      NullLogger<CertStatusHandler>.Instance, CreateOptions(), db);

    var result = await handler.GetStatusAsync(CreatePrincipal(null), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Unauthorized", result.Error.Code);
  }

  [Fact]
  public async Task GetStatusAsync_ReturnsAgentNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new CertStatusHandler(
      NullLogger<CertStatusHandler>.Instance, CreateOptions(), db);

    var result = await handler.GetStatusAsync(CreatePrincipal("PC-001"), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task GetStatusAsync_ReturnsCertificateNotFound_WhenNoCertificate()
  {
    using var db = DbContextFactory.Create();
    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new CertStatusHandler(
      NullLogger<CertStatusHandler>.Instance, CreateOptions(), db);

    var result = await handler.GetStatusAsync(CreatePrincipal("PC-001"), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task GetStatusAsync_ReturnsStatus_WithCorrectFields()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    db.Certificates.Add(CreateActiveCert(agent.Id));
    await db.SaveChangesAsync();

    var handler = new CertStatusHandler(
      NullLogger<CertStatusHandler>.Instance, CreateOptions(), db);

    var result = await handler.GetStatusAsync(CreatePrincipal("PC-001"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("serial-1", result.Value.SerialNumber);
    Assert.Equal("thumb-1", result.Value.Thumbprint);
    Assert.Equal("CN=PC-001", result.Value.SubjectName);
    Assert.True(result.Value.IsValid);
    Assert.Null(result.Value.RevokedAt);
  }

  [Fact]
  public async Task GetStatusAsync_SetsNeedsRenewal_WhenCertificateIsNearExpiry()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    // Cert expiring in 10 days, threshold is 30 days → needs renewal
    db.Certificates.Add(CreateActiveCert(agent.Id, DateTimeOffset.UtcNow.AddDays(10)));
    await db.SaveChangesAsync();

    var handler = new CertStatusHandler(
      NullLogger<CertStatusHandler>.Instance, CreateOptions(renewalThresholdDays: 30), db);

    var result = await handler.GetStatusAsync(CreatePrincipal("PC-001"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.NeedsRenewal);
  }

  [Fact]
  public async Task GetStatusAsync_DoesNotSetNeedsRenewal_WhenCertificateIsNotNearExpiry()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    // Cert expiring in 90 days, threshold is 30 days → no renewal needed
    db.Certificates.Add(CreateActiveCert(agent.Id, DateTimeOffset.UtcNow.AddDays(90)));
    await db.SaveChangesAsync();

    var handler = new CertStatusHandler(
      NullLogger<CertStatusHandler>.Instance, CreateOptions(renewalThresholdDays: 30), db);

    var result = await handler.GetStatusAsync(CreatePrincipal("PC-001"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(result.Value.NeedsRenewal);
  }
}
