using AuthService.Interfaces;
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
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> AuthenticateUser(User user)
        {
            if (user == null) return false;
            var existingUser = await _userRepository.GetByEmailAsync(user.Email);
            return existingUser != null && existingUser.Password == user.Password;
        }

        public async Task<bool> CreateUser(User user)
        {
            if (user == null || await _userRepository.CheckForEmailConflictAsync(user.Email)) return false;
            await _userRepository.AddAsync(user);
            return true;
        }

        public async Task<bool> DeleteUser(User user)
        {
            if (user == null) return false;
            await _userRepository.DeleteAsync(user.Id);
            return true;
        }
    }
}
