using Server.Api.Infrastructure;
using Xunit;

namespace Server.Api.Tests.Infrastructure;

public class HmacDataHasherTests
{
  private readonly HmacDataHasher _hasher = new();

  [Fact]
  public void CreateDataHash_ReturnsNonEmptyHashAndSalt()
  {
    var (hash, salt) = _hasher.CreateDataHash("password123");

    Assert.NotEmpty(hash);
    Assert.NotEmpty(salt);
  }

  [Fact]
  public void CreateDataHash_ProducesDifferentSaltsEachCall()
  {
    var (_, salt1) = _hasher.CreateDataHash("password");
    var (_, salt2) = _hasher.CreateDataHash("password");

    Assert.False(salt1.SequenceEqual(salt2));
  }

  [Fact]
  public void CreateDataHash_ProducesDifferentHashesEachCall()
  {
    var (hash1, _) = _hasher.CreateDataHash("password");
    var (hash2, _) = _hasher.CreateDataHash("password");

    Assert.False(hash1.SequenceEqual(hash2));
  }

  [Fact]
  public void IsDataValid_ReturnsTrue_WhenPasswordMatchesHash()
  {
    var (hash, salt) = _hasher.CreateDataHash("correctPassword");

    var result = _hasher.IsDataValid("correctPassword", hash, salt);

    Assert.True(result);
  }

  [Fact]
  public void IsDataValid_ReturnsFalse_WhenPasswordDoesNotMatch()
  {
    var (hash, salt) = _hasher.CreateDataHash("correctPassword");

    var result = _hasher.IsDataValid("wrongPassword", hash, salt);

    Assert.False(result);
  }

  [Fact]
  public void IsDataValid_ReturnsFalse_WhenHashIsModified()
  {
    var (hash, salt) = _hasher.CreateDataHash("password");
    hash[0] ^= 0xFF;

    var result = _hasher.IsDataValid("password", hash, salt);

    Assert.False(result);
  }

  [Fact]
  public void CreateDataHash_HashLengthIs64Bytes()
  {
    var (hash, _) = _hasher.CreateDataHash("test");

    Assert.Equal(64, hash.Length);
  }
}
