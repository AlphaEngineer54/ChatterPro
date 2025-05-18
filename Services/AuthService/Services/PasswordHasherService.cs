using AuthService.Interfaces;
using Isopoh.Cryptography.Argon2;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services
{
    public class PasswordHasherService : IPasswordHasher
    {
        public string Hash(string password)
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 10,
                MemoryCost = 65536, // 64 MB
                Lanes = 4,
                Threads = Environment.ProcessorCount,
                HashLength = 32,
                Password = Encoding.UTF8.GetBytes(password),  // **Indispensable**
                Salt = GenerateSalt()            // Génération automatique du sel
            };

            using var argon2 = new Argon2(config);
            var hash = argon2.Hash();

            return config.EncodeString(hash.Buffer);
        }

        public bool Verify(string dbPassword, string password)
        {
            return Argon2.Verify(dbPassword, password);
        }
        public static byte[] GenerateSalt(int length = 16)
        {
            var salt = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
