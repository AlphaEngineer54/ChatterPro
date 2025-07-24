using UserService.Interfaces;
using UserService.Events;
using UserService.Models;
using Microsoft.AspNetCore.SignalR;
using UserService.Hubs;

namespace UserService.Services
{

    public class MultiEventHandler : IEventHandler
    {
        private readonly AccountService _userService;
        private readonly ILogger<MultiEventHandler> _logger;
        private readonly IHubContext<UserHub> _hubs;

        public MultiEventHandler(AccountService userService, 
                                 ILogger<MultiEventHandler> logger,
                                 IHubContext<UserHub> hubs)
        {
            this._userService = userService;
            this._logger = logger;
            this._hubs = hubs;
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
                        var users = await GetUsers(getUserIEvent);
                        await this._hubs.Clients.User(getUserIEvent.UserId.ToString())
                                                .SendAsync("ReceiveUsers", users);

                        _logger.LogInformation($"Users for UserId {getUserIEvent.UserId} have been sent successfully.");
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