using AuthService.Interfaces;
using Isopoh.Cryptography.Argon2;

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
                MemoryCost = 65536, // Augmente la résistance contre les attaques GPU
                Lanes = 4,
                Threads = Environment.ProcessorCount,
                HashLength = 32
            };

            using var argon2 = new Argon2(config);
            using var hash = argon2.Hash();

            return config.EncodeString(hash.Buffer);
        }

        public bool Verify(string dbPassword, string password)
        {
            return Argon2.Verify(dbPassword, password);
        }
    }
}
