using AuthService.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;

namespace AuthService.Services
{
    /// <summary>
    /// Service d'authentification pour gérer la logique la métier de l'authentification uniquement
    /// </summary>
    /// <param name="context"></param>
    public class UserService(AuthDbContext context)
    {
        private readonly AuthDbContext _context = context;

        /// <summary>
        /// Authentiie un utilisateur via son email et son mot de passe
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> AuthenficateUser(User user)
        {
            if (user == null) { return false; }

            var existingUser = await this._context.Users.FirstOrDefaultAsync(u => u.Email == user.Email &&
                                                                                  u.Password == user.Password);

            return existingUser != null;
        }

        /// <summary>
        /// Créer un nouvel utilisateur dans la base de données
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> CreateUser(User user)
        {
            if (user == null || await CheckForEmailCnflict(user)) { return false; }

            await _context.Users.AddAsync(user);

            var result = await this._context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteUser(User user)
        {
            var foundUser = await this._context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            if (foundUser == null) 
            { 
                return false; 
            }

            this._context.Users.Remove(foundUser);
            await this._context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckForEmailConflict(User user)
        {
            var result = await this._context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            return result != null; // Si le resultat n'est pas nul, un email est en conflit avec un autre user.
        }
    }
}
