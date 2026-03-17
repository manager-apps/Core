using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Server.Domain;
using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Result;
using Server.Ingest.Features.Cert;
using Server.Ingest.Features.Cert.Renew;
using Server.Ingest.Tests.Helpers;
using Xunit;

namespace Server.Ingest.Tests.Features.Cert;

public class CertRenewHandlerTests
{
  private static ClaimsPrincipal CreatePrincipal(string? agentName)
  {
    var identity = agentName is null
      ? new ClaimsIdentity()
      : new ClaimsIdentity([new Claim(ClaimTypes.Name, agentName)], "Certificate");
    return new ClaimsPrincipal(identity);
  }

  private static ICertService CreateCertServiceMock()
  {
    var certService = Substitute.For<ICertService>();
    var response = new CertEnrollResponse("new-cert-pem", "ca-pem", DateTimeOffset.UtcNow.AddYears(1));
    certService.IssueCertificateAsync(
        Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result<CertEnrollResponse>.Success(response)));
    return certService;
  }

  private static Certificate CreateActiveCert(long agentId)
    => Certificate.Create(agentId, "serial-1", "thumbprint-1", "CN=TestAgent",
      DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

  [Fact]
  public async Task RenewAsync_ReturnsUnauthorized_WhenAgentNameIsEmpty()
  {
    using var db = DbContextFactory.Create();
    var handler = new CertRenewHandler(
      NullLogger<CertRenewHandler>.Instance, CreateCertServiceMock(), db);

    var result = await handler.RenewAsync(
      CreatePrincipal(null), new CertRenewRequest("csr-pem"), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Unauthorized", result.Error.Code);
  }

  [Fact]
  public async Task RenewAsync_ReturnsAgentNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new CertRenewHandler(
      NullLogger<CertRenewHandler>.Instance, CreateCertServiceMock(), db);

    var result = await handler.RenewAsync(
      CreatePrincipal("PC-001"), new CertRenewRequest("csr-pem"), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task RenewAsync_ReturnsRenewalNotAllowed_WhenNoActiveCertificate()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var handler = new CertRenewHandler(
      NullLogger<CertRenewHandler>.Instance, CreateCertServiceMock(), db);

    var result = await handler.RenewAsync(
      CreatePrincipal("PC-001"), new CertRenewRequest("csr-pem"), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Forbidden", result.Error.Code);
  }

  [Fact]
  public async Task RenewAsync_CallsCertService_WhenAgentHasActiveCertificate()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    db.Certificates.Add(CreateActiveCert(agent.Id));
    await db.SaveChangesAsync();

    var certService = CreateCertServiceMock();
    var handler = new CertRenewHandler(
      NullLogger<CertRenewHandler>.Instance, certService, db);

    var result = await handler.RenewAsync(
      CreatePrincipal("PC-001"), new CertRenewRequest("new-csr"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("new-cert-pem", result.Value.CertificatePem);
    await certService.Received(1).IssueCertificateAsync(
      agent.Id, "PC-001", "new-csr", Arg.Any<CancellationToken>());
  }
}
