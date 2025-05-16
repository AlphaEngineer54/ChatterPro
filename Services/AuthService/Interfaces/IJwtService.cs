using AuthService.Models;

namespace AuthService.Interfaces
{
    public interface IJwtService
    {
        string GenerateJWTToken(User user);
    }
}
