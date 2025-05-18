using Microsoft.EntityFrameworkCore;
using UserService.Events;
using UserService.Interfaces;
using UserService.Models;
using UserService.Models.DTO.User;

namespace UserService.Services
{
    public class AccountService(UserDbContext context, ILogger<AccountService> logger, IProducer producer)
    {
        private readonly UserDbContext _context = context;
        private readonly ILogger<AccountService> _logger = logger;
        private readonly IProducer producer = producer;

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

        public async Task<bool> UpdateUser(UserUpdateDTO user)
        {
            if (user == null || !await CheckIfUserExist(user.Id)) { return false; }

            try
            {
                var updatedUser = new User()
                {
                    Id = user.Id,
                    UserName = user.UserName
                };

                this._context.Update(updatedUser);  
                
                await this._context.SaveChangesAsync();

                this.producer.SendEvent(new UserUpdatedEvent() { 
                    Id = user.Id,
                    Email = user.Email,
                    Password = user.Password,
                },"user-updated");

                this._logger.LogInformation($"User updated successfully into database: User-{user.Id}");
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError($"An error occured while updating the user:{ex.Message}");
                return false;
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
