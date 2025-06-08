using System.Security.Cryptography;
using System.Text;

public class PasswordHasher
{
    private readonly string _encryptionKey;

    public PasswordHasher(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    public (string Hash, string Salt) HashPassword(string password)
    {
        // Generate a random salt
        var saltBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);

        // Hash password with salt and encryption key
        var hash = ComputeHash(password, salt);
        return (hash, salt);
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var hash = ComputeHash(password, storedSalt);
        return hash == storedHash;
    }

    private string ComputeHash(string password, string salt)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        // Combine password and salt
        var toHash = new byte[passwordBytes.Length + saltBytes.Length];
        Buffer.BlockCopy(passwordBytes, 0, toHash, 0, passwordBytes.Length);
        Buffer.BlockCopy(saltBytes, 0, toHash, passwordBytes.Length, saltBytes.Length);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(toHash);
        return Convert.ToBase64String(hashBytes);
    }
}