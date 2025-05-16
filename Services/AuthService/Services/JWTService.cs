using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("5f2d6ab9e4c8210fd8c7a3f91b6e72adc4f9137e25ab8d3c6eaf7b014f29c3db")); // Utilisation d'une clé dynamique

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
