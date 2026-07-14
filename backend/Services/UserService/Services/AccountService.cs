using Microsoft.EntityFrameworkCore;
using UserService.Events;
using UserService.Interfaces;
using UserService.Models;
using UserService.Models.DTO.User;

namespace UserService.Services
{
    public class AccountService(UserDbContext context, ILogger<AccountService> logger, IProducer producer, AccountUpdateSaga saga)
    {
        private readonly UserDbContext _context = context;
        private readonly ILogger<AccountService> _logger = logger;
        private readonly IProducer producer = producer;
        private readonly AccountUpdateSaga _saga = saga;

        public async Task AddUser(User user)
        {
            if (user == null) { return; }

            try
            {
                await this._context.AddAsync(user);
                await this._context.SaveChangesAsync();

                this._logger.LogInformation($"User added successfully into database: User-{user.Id}");
            }
            catch(Exception ex)
            {
                this._logger.LogError($"An error occured while adding new user:{ex.Message}");
            }
        }

        public async Task<bool> DeleteUser(User user)
        {
            if (user == null) { return false; }

            try
            {
                this._context.Remove(user);
                await this._context.SaveChangesAsync();

                // Envoyer un event pour informer d'autres services de la suppression d'un utilisateur
                this.producer.SendEvent(new UserDeletedEvent() {  Id = user.Id  }, "user-deleted"); 

                this._logger.LogInformation($"User removed successfully from database: User-{user.Id}");
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError($"An error occured while removing user:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Orchestre la mise à jour de compte via un SAGA : AuthService valide d'abord
        /// l'identité (mot de passe) et applique l'email ; en cas de confirmation, on
        /// commit localement le pseudo. Si ce commit local échoue, on compense côté
        /// AuthService (restauration de l'email). Le mot de passe n'est jamais modifié.
        /// </summary>
        public async Task<AccountUpdateResult> UpdateUser(UserUpdateDTO user)
        {
            if (user == null || !await CheckIfUserExist(user.Id)) { return AccountUpdateResult.NotFound; }

            var sagaId = Guid.NewGuid();
            var reply = await this._saga.RequestAsync(new AccountUpdateRequested
            {
                SagaId = sagaId,
                UserId = user.Id,
                NewEmail = user.Email,
                AuthPassword = user.Password
            }, TimeSpan.FromSeconds(8));

            var status = Enum.TryParse<AccountUpdateStatus>(reply?.Status, out var parsed)
                ? parsed
                : AccountUpdateStatus.Error;

            switch (status)
            {
                case AccountUpdateStatus.Confirmed:
                    try
                    {
                        // Étape locale (réversible) : seul le pseudo vit dans UserService.
                        this._context.Update(new User { Id = user.Id, UserName = user.UserName });
                        await this._context.SaveChangesAsync();

                        this._logger.LogInformation($"User updated successfully into database: User-{user.Id}");
                        return AccountUpdateResult.Ok;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError($"Local commit failed, compensating saga {sagaId}: {ex.Message}");
                        await this._saga.PublishCompensateAsync(sagaId);
                        return AccountUpdateResult.Error;
                    }

                case AccountUpdateStatus.InvalidCredentials:
                    return AccountUpdateResult.InvalidCredentials;
                case AccountUpdateStatus.EmailConflict:
                    return AccountUpdateResult.EmailConflict;
                case AccountUpdateStatus.NotFound:
                    return AccountUpdateResult.NotFound;
                default:
                    this._logger.LogError($"Saga {sagaId} failed: {reply?.Message}");
                    return AccountUpdateResult.Error;
            }
        }

        public async Task<User> GetUserByUserName(string userName)
        {
            var foundUser = await this._context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (foundUser != null)
            {
                this._logger.LogInformation($"User {userName} found and send to client");
                return foundUser;
            }

            this._logger.LogError($"User {userName} was not found.");
            return null;
       }
        public async Task<User> GetUserInfo(int id)
        {
            try
            {
                var foundUser = await this._context.Users.FirstOrDefaultAsync(u => u.Id == id);
                
                if(foundUser != null)
                {
                    this._logger.LogInformation($"User {id} found and send to client");
                    return foundUser;   
                }
            }
            catch(Exception ex)
            {
                this._logger.LogError($"User {id} was not found. Error: {ex.Message}");
            }

            return null;
        }

        private async Task<bool> CheckIfUserExist(int id)
        {
            return await this._context.Users.AnyAsync(u => u.Id == id);
        }
    }
}
