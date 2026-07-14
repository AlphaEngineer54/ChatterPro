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

        public async Task UpdateUser(User user)
        {
            if (await _userRepository.CheckForEmailConflictAsync(user.Email))
                throw new InvalidCredentialException($"this email '{user.Email}' already exist");

            if (await _userRepository.GetByIdAsync(user.Id) == null)
                throw new ArgumentNullException($"User with ID {user.Id} not found. Cannot update.");

            await this._userRepository.UpdateAsync(new User
            {
                Id = user.Id,
                Email = user.Email!,
                Password = _passwordHasher.Hash(user.Password!),
            });
        }

        public async Task<bool> DeleteUser(User user)
        {
            if (await _userRepository.GetByIdAsync(user.Id) == null) return false;
            await _userRepository.DeleteAsync(user.Id);
            return true;
        }  

        public async Task<User> GetUserById(int id)
        {
            return await _userRepository.GetByIdAsync(id)
                   ?? throw new ArgumentNullException($"User with ID {id} not found.");
        }

        /// <summary>
        /// Étape « valider &amp; préparer » du SAGA de mise à jour de compte.
        /// Vérifie que <paramref name="authPassword"/> correspond bien au mot de passe
        /// actuel (preuve d'identité), contrôle l'unicité de l'email (en excluant
        /// l'utilisateur lui-même), puis applique le nouvel email — le mot de passe
        /// restant inchangé. Renvoie l'issue et l'ancien email (pour compensation).
        /// </summary>
        public async Task<(AccountUpdateOutcome Outcome, string? OldEmail)> PrepareAccountUpdate(
            int userId, string? newEmail, string? authPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (AccountUpdateOutcome.NotFound, null);

            // Le vrai filtre de sécurité qui manquait : on prouve l'identité avant tout.
            if (string.IsNullOrEmpty(authPassword) || !_passwordHasher.Verify(user.Password!, authPassword))
                return (AccountUpdateOutcome.InvalidCredentials, null);

            var oldEmail = user.Email;

            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != oldEmail)
            {
                // Conflit uniquement si l'email appartient à un AUTRE utilisateur.
                var owner = await _userRepository.GetByEmailAsync(newEmail);
                if (owner != null && owner.Id != userId)
                    return (AccountUpdateOutcome.EmailConflict, null);

                // Entité suivie (FindAsync) : on ne touche qu'à l'email, le hash reste intact.
                user.Email = newEmail;
                await _userRepository.UpdateAsync(user);
            }

            return (AccountUpdateOutcome.Confirmed, oldEmail);
        }

        /// <summary>
        /// Compensation du SAGA : restaure l'email d'origine (le mot de passe n'ayant
        /// jamais été modifié, il n'y a rien à compenser de ce côté).
        /// </summary>
        public async Task RevertAccountEmail(int userId, string oldEmail)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.Email = oldEmail;
            await _userRepository.UpdateAsync(user);
        }
    }
}
