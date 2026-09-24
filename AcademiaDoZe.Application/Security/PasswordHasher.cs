using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Application.Security;

public class PasswordHasher : IPasswordHasher
{
    // Parâmetros padrão recomendados
    private const int DefaultIterations = 3; // t
    private const int DefaultMemoryKb = 65536; // 64 MB
    private const int DefaultParallelism = 1; // p
    private const int HashLength = 32; // bytes
    private const int SaltLength = 16; // bytes

    public string Hash(string password)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        var salt = new byte[SaltLength];
        RandomNumberGenerator.Fill(salt);

        var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DefaultParallelism,
            Iterations = DefaultIterations,
            MemorySize = DefaultMemoryKb
        };

        var hash = argon.GetBytes(HashLength);

        string saltB64 = Convert.ToBase64String(salt);
        string hashB64 = Convert.ToBase64String(hash);

        return $"ARGON2ID:{DefaultIterations}:{DefaultMemoryKb}:{DefaultParallelism}:{saltB64}:{hashB64}";
    }

    public bool Verify(string password, string passwordHash)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));
        if (passwordHash == null) throw new ArgumentNullException(nameof(passwordHash));

        // Expected format: ARGON2ID:{t}:{mKb}:{p}:{saltBase64}:{hashBase64}
        var parts = passwordHash.Split(':');
        if (parts.Length != 6 || !parts[0].Equals("ARGON2ID", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!int.TryParse(parts[1], out var iterations)) return false;
        if (!int.TryParse(parts[2], out var memoryKb)) return false;
        if (!int.TryParse(parts[3], out var parallelism)) return false;

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[4]);
            expectedHash = Convert.FromBase64String(parts[5]);
        }
        catch
        {
            return false;
        }

        var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            Iterations = iterations,
            MemorySize = memoryKb
        };

        var computed = argon.GetBytes(expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(computed, expectedHash);
    }
}
