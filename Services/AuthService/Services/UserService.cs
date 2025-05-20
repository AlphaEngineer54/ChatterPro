using AuthService.Interfaces;
using AuthService.Models;
using System.Security.Authentication;

namespace AuthService.Services
{
    /// <summary>
    /// Service d'authentification pour gérer la logique la métier de l'authentification uniquement
    /// </summary>
    /// <param name="context"></param>  
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> AuthenticateUser(User user)
        {
            var existingUser = await _userRepository.GetByEmailAsync(user.Email) 
                                    ?? throw new InvalidCredentialException($"Email is incorrect"); 

            if (!_passwordHasher.Verify(existingUser.Password, user.Password)) 
                throw new InvalidCredentialException("Password is incorrect");

            return existingUser;
        }

        public async Task<User> CreateUser(User user)
        {
            if(await _userRepository.CheckForEmailConflictAsync(user.Email)) 
                throw new InvalidCredentialException($"this email '{user.Email}' already exist");
                
            user.Password = _passwordHasher.Hash(user.Password);
            await _userRepository.AddAsync(user);
            return user;
        }

        public async Task<bool> DeleteUser(User user)
        {
            if (user == null) return false;
            await _userRepository.DeleteAsync(user.Id);
            return true;
        }  
    }
}
