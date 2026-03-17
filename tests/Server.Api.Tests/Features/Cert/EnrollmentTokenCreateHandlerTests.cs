using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using Server.Api.Common.Interfaces;
using Server.Api.Features.Cert;
using Server.Api.Features.Cert.Create;
using Server.Api.Tests.Helpers;

namespace Server.Api.Tests.CertHandlers;

public class EnrollmentTokenCreateHandlerTests
{
  private static IDataHasher CreateHasherMock()
  {
    var hasher = Substitute.For<IDataHasher>();
    hasher.CreateDataHash(Arg.Any<string>())
      .Returns((new byte[32], new byte[32]));
    return hasher;
  }

  [Fact]
  public async Task HandleAsync_CreatesTokenAndPersistsToDatabase()
  {
    using var db = DbContextFactory.Create();
    var handler = new EnrollmentTokenCreateHandler(
      NullLogger<EnrollmentTokenCreateHandler>.Instance,
      CreateHasherMock(),
      db);

    var request = new CreateEnrollmentTokenRequest("PC-001", 24);

    var result = await handler.HandleAsync(request, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("PC-001", result.Value.AgentName);
    Assert.NotEmpty(result.Value.Token);
    Assert.True(result.Value.ExpiresAt > DateTimeOffset.UtcNow);
  }

  [Fact]
  public async Task HandleAsync_StoredTokenExpiresAfterSpecifiedHours()
  {
    using var db = DbContextFactory.Create();
    var handler = new EnrollmentTokenCreateHandler(
      NullLogger<EnrollmentTokenCreateHandler>.Instance,
      CreateHasherMock(),
      db);

    var before = DateTimeOffset.UtcNow.AddHours(47);
    var request = new CreateEnrollmentTokenRequest("PC-002", 48);

    var result = await handler.HandleAsync(request, CancellationToken.None);
    var after = DateTimeOffset.UtcNow.AddHours(49);

    Assert.True(result.Value.ExpiresAt >= before);
    Assert.True(result.Value.ExpiresAt <= after);
  }

  [Fact]
  public async Task HandleAsync_SavesEnrollmentTokenToDatabase()
  {
    using var db = DbContextFactory.Create();
    var handler = new EnrollmentTokenCreateHandler(
      NullLogger<EnrollmentTokenCreateHandler>.Instance,
      CreateHasherMock(),
      db);

    await handler.HandleAsync(new CreateEnrollmentTokenRequest("PC-003", 24), CancellationToken.None);

    var saved = db.EnrollmentTokens.Single();
    Assert.Equal("PC-003", saved.AgentName);
    Assert.False(saved.IsUsed);
  }
}
