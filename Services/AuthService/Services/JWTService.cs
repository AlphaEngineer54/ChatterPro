using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Interfaces;
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public class JWTService : IJwtService
    {
        public string GenerateJWTToken(User user)
        {
            // Définir les "claims" pour l'utilisateur
            var claims = new[]
            {
              new Claim(ClaimTypes.Name, user.Id.ToString() ?? ""),
              new Claim(ClaimTypes.Email, user.Email ?? ""),
              new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
           };
            
            // Récupérer la clé secrète depuis la configuration sécurisée (ex : appsettings.json, variables d'environnement, etc.)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") 
                                                                                  ?? throw new ArgumentException("JWT Super Key was not set"))); 

            // Définir les credentials de signature avec la clé et l'algorithme
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Créer un token avec les paramètres requis
            var token = new JwtSecurityToken(
                issuer: "http://localhost:5000",
                audience: "messaging_api",     
                claims: claims,                 
                expires: DateTime.UtcNow.AddHours(1), 
                signingCredentials: creds     
            );

            // Retourner le token sous forme de chaîne de caractères
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
