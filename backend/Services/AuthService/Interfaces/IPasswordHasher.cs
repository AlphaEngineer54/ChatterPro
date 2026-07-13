namespace AuthService.Interfaces
{
    public interface IPasswordHasher
    {
        public string Hash(string password);
        public bool Verify(string dbPassword, string password);
    }
}
