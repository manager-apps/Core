namespace Server.Application.Abstractions.Providers;

public interface IPasswordHasher
{
  bool IsPasswordValid(string password, byte[] storedHash, byte[] storedSalt);


  (byte[] hash, byte[] salt) CreatePasswordHash(string password);
}
