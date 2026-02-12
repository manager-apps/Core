namespace Server.Ingest.Common.Interfaces;

public interface IDataHasher
{
  /// <summary>
  /// Validates a password against a stored hash and salt.
  /// </summary>
  /// <param name="password"></param>
  /// <param name="storedHash"></param>
  /// <param name="storedSalt"></param>
  /// <returns></returns>
  bool IsDataValid(string password, byte[] storedHash, byte[] storedSalt);

  /// <summary>
  /// Creates a password hash and salt for a given password.
  /// </summary>
  /// <param name="password"></param>
  /// <returns></returns>
  (byte[] hash, byte[] salt) CreateDataHash(string password);
}

