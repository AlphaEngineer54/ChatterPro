using UserService.Interfaces;
using UserService.Events;
using UserService.Models;

namespace UserService.Services
{

    public class MultiEventHandler : IEventHandler
    {
        private readonly AccountService _userService;
        private readonly ILogger<MultiEventHandler> _logger;

        public MultiEventHandler(AccountService userService, ILogger<MultiEventHandler> logger)
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
                    case UserCreatedEvent userCreatedEvent:
                        await HandleUserCreatedEvent(userCreatedEvent);
                        break;
                    case GetUserIEvent getUserIEvent:
                        // TODO : Utiliser SignalR pour renvoyer les données au client
                        var users = await GetUsers(getUserIEvent);
                        this._logger.LogInformation($"Users found : {users.Count}");  
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

        private async Task HandleUserCreatedEvent(UserCreatedEvent eventMessage)
        {
            var user = new User()
            {
                Id = eventMessage.Id,
                UserName = eventMessage.UserName ?? "unknown-user"
            };

            await this._userService.AddUser(user);
        }

        private async Task<IList<User>> GetUsers(GetUserIEvent getUserIEvent)
        {
            var users = new List<User>();

            foreach (var id in getUserIEvent.ids)
            {
                var user = await _userService.GetUserInfo(id);

                if (user != null)
                {
                    users.Add(user);
                }
            }

            return users;
        }
    }
}