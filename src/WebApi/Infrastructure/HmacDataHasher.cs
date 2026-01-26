using WebApi.Common.Interfaces;

namespace WebApi.Infrastructure;

public class HmacDataHasher : IDataHasher
{
  public bool IsDataValid(string password, byte[] storedHash, byte[] storedSalt)
  {
    using var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt);
    var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    return computedHash.SequenceEqual(storedHash);
  }

  public (byte[] hash, byte[] salt) CreateDataHash(string password)
  {
    using var hmac = new System.Security.Cryptography.HMACSHA512();
    var salt = hmac.Key;
    var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    return (hash, salt);
  }
}
