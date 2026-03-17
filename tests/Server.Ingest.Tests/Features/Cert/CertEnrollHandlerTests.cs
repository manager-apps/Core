using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Server.Domain;
using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Result;
using Server.Ingest.Features.Cert;
using Server.Ingest.Features.Cert.Enroll;
using Server.Ingest.Tests.Helpers;
using Xunit;

namespace Server.Ingest.Tests.Features.Cert;

public class CertEnrollHandlerTests
{
  private static IDataHasher CreateHasherMock(bool valid = true)
  {
    var hasher = Substitute.For<IDataHasher>();
    hasher.IsDataValid(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<byte[]>())
      .Returns(valid);
    return hasher;
  }

  private static ICertService CreateCertServiceMock()
  {
    var certService = Substitute.For<ICertService>();
    var response = new CertEnrollResponse("cert-pem", "ca-pem", DateTimeOffset.UtcNow.AddYears(1));
    certService.IssueCertificateAsync(
        Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result<CertEnrollResponse>.Success(response)));
    return certService;
  }

  private static EnrollmentToken CreateValidToken(string agentName)
    => EnrollmentToken.Create(agentName, new byte[32], new byte[32], TimeSpan.FromHours(24));

  [Fact]
  public async Task EnrollWithTokenAsync_ReturnsError_WhenNoTokensExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new CertEnrollHandler(
      NullLogger<CertEnrollHandler>.Instance,
      CreateCertServiceMock(),
      CreateHasherMock(),
      db);

    var request = new TokenEnrollRequest("PC-001", "tag1", "csr-pem", "some-token");

    var result = await handler.EnrollWithTokenAsync(request, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Unauthorized", result.Error.Code);
  }

  [Fact]
  public async Task EnrollWithTokenAsync_ReturnsError_WhenTokenIsAlreadyUsed()
  {
    using var db = DbContextFactory.Create();
    var token = CreateValidToken("PC-001");
    token.MarkAsUsed(1);
    db.EnrollmentTokens.Add(token);
    await db.SaveChangesAsync();

    var handler = new CertEnrollHandler(
      NullLogger<CertEnrollHandler>.Instance,
      CreateCertServiceMock(),
      CreateHasherMock(),
      db);

    var result = await handler.EnrollWithTokenAsync(
      new TokenEnrollRequest("PC-001", "tag1", "csr-pem", "some-token"),
      CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Unauthorized", result.Error.Code);
  }

  [Fact]
  public async Task EnrollWithTokenAsync_ReturnsError_WhenDataHasherRejectsToken()
  {
    using var db = DbContextFactory.Create();
    db.EnrollmentTokens.Add(CreateValidToken("PC-001"));
    await db.SaveChangesAsync();

    var handler = new CertEnrollHandler(
      NullLogger<CertEnrollHandler>.Instance,
      CreateCertServiceMock(),
      CreateHasherMock(valid: false),
      db);

    var result = await handler.EnrollWithTokenAsync(
      new TokenEnrollRequest("PC-001", "tag1", "csr-pem", "wrong-token"),
      CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Unauthorized", result.Error.Code);
  }

  [Fact]
  public async Task EnrollWithTokenAsync_CreatesNewAgent_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    db.EnrollmentTokens.Add(CreateValidToken("PC-001"));
    await db.SaveChangesAsync();

    var handler = new CertEnrollHandler(
      NullLogger<CertEnrollHandler>.Instance,
      CreateCertServiceMock(),
      CreateHasherMock(),
      db);

    await handler.EnrollWithTokenAsync(
      new TokenEnrollRequest("PC-001", "prod", "csr-pem", "token"),
      CancellationToken.None);

    var agent = db.Agents.Single();
    Assert.Equal("PC-001", agent.Name);
    Assert.Equal("prod", agent.SourceTag);
  }

  [Fact]
  public async Task EnrollWithTokenAsync_UsesExistingAgent_WhenAgentAlreadyExists()
  {
    using var db = DbContextFactory.Create();
    db.Agents.Add(Agent.Create("PC-001", "prod"));
    db.EnrollmentTokens.Add(CreateValidToken("PC-001"));
    await db.SaveChangesAsync();

    var handler = new CertEnrollHandler(
      NullLogger<CertEnrollHandler>.Instance,
      CreateCertServiceMock(),
      CreateHasherMock(),
      db);

    await handler.EnrollWithTokenAsync(
      new TokenEnrollRequest("PC-001", "prod", "csr-pem", "token"),
      CancellationToken.None);

    Assert.Equal(1, db.Agents.Count());
  }

  [Fact]
  public async Task EnrollWithTokenAsync_MarksTokenAsUsed_OnSuccess()
  {
    using var db = DbContextFactory.Create();
    db.EnrollmentTokens.Add(CreateValidToken("PC-001"));
    await db.SaveChangesAsync();

    var handler = new CertEnrollHandler(
      NullLogger<CertEnrollHandler>.Instance,
      CreateCertServiceMock(),
      CreateHasherMock(),
      db);

    await handler.EnrollWithTokenAsync(
      new TokenEnrollRequest("PC-001", "tag1", "csr-pem", "token"),
      CancellationToken.None);

    var token = db.EnrollmentTokens.Single();
    Assert.True(token.IsUsed);
  }

  [Fact]
  public async Task EnrollWithTokenAsync_ReturnsSuccess_WithCertResponse()
  {
    using var db = DbContextFactory.Create();
    db.EnrollmentTokens.Add(CreateValidToken("PC-001"));
    await db.SaveChangesAsync();

    var handler = new CertEnrollHandler(
      NullLogger<CertEnrollHandler>.Instance,
      CreateCertServiceMock(),
      CreateHasherMock(),
      db);

    var result = await handler.EnrollWithTokenAsync(
      new TokenEnrollRequest("PC-001", "tag1", "csr-pem", "token"),
      CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("cert-pem", result.Value.CertificatePem);
    Assert.Equal("ca-pem", result.Value.CaCertificatePem);
  }
}
