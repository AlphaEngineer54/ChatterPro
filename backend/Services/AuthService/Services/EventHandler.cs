using AuthService.Events;
using AuthService.Interfaces;
using AuthService.Models;

namespace AuthService.Services
{
    public class EventHandlerService : IEventHandler
    {
        private readonly UserService _userService;
        private readonly ILogger<EventHandlerService> _logger;

        public EventHandlerService(UserService userService, ILogger<EventHandlerService> logger)
        {
            this._userService = userService;
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
            finally
            {
                _logger.LogInformation($"Event {eventMessage} has been processed successfully.");
            }
        }


        public async Task DeleteUser(int id)
        {
            var user = await this._userService.GetUserById(id);
            await this._userService.DeleteUser(user);

            this._logger.LogInformation($"User-{id} has been removed successfully from Auth-Database!");
        }

        public async Task UpdateUser(UserUpdatedEvent user)
        {
            await this._userService.UpdateUser(new User
            {
                Id = user.Id,
                Email = user.Email,
                Password = user.Password
            });

            this._logger.LogInformation($"User-{user.Id} has been updated successfully!");
        }

    }
}
