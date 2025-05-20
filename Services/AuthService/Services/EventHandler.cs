using AuthService.Events;
using AuthService.Interfaces;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class EventHandlerService : IEventHandler
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<EventHandlerService> _logger;

        public EventHandlerService(AuthDbContext context, ILogger<EventHandlerService> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        public async Task HandleEventAsync(Event eventMessage)
        {
            if (eventMessage == null)
            {
                _logger.LogWarning("Received a null event message.");
                return;
            }

            try
            {
                // Gestion des événements selon leur type
                // Actuellement, il y a deux events. Selon les nouveaux besoins, nous pouvons ajouter d'autres events
                switch (eventMessage)
                {
                    case UserUpdatedEvent updated:
                        await UpdateUser(updated);
                        break;
                    case UserDeletedEvent deleted:
                        await DeleteUser(deleted.Id);
                        break;
                    default:
                        _logger.LogWarning($"Unhandled event type: {eventMessage.GetType()}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while handling the event: {ex.Message}");
            }
        }


        public async Task DeleteUser(int id)
        {
             var foundUser = await this._context.Users.FirstOrDefaultAsync(x => x.Id == id);

             if (foundUser == null) { throw new ArgumentException($"Unable to delete the user with ID {id}"); }

            this._context.Users.Remove(foundUser);
            await this._context.SaveChangesAsync();

            this._logger.LogInformation($"User-{id} has been removed successfully from Auth-Database!");
        }

        public async Task UpdateUser(UserUpdatedEvent user)
        {
            var foundUser = await this._context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);

            if (foundUser == null) { throw new ArgumentException("Updated-User-Event is null!"); };

            foundUser.Email = user.Email;
            foundUser.Password = user.Password;

            await this._context.SaveChangesAsync();

            this._logger.LogInformation($"User-{user.Id} has been updated successfully!");
        }

    }
}
