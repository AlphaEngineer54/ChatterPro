using AuthService.Events;
using AuthService.Interfaces;
using AuthService.Models;

namespace AuthService.Services
{
    public class EventHandlerService : IEventHandler
    {
        private const string ReplyQueue = "account-update-reply";

        private readonly UserService _userService;
        private readonly ILogger<EventHandlerService> _logger;
        private readonly IProducer _producer;
        private readonly AccountUpdateSnapshotStore _snapshots;

        public EventHandlerService(UserService userService,
                                   ILogger<EventHandlerService> logger,
                                   IProducer producer,
                                   AccountUpdateSnapshotStore snapshots)
        {
            this._userService = userService;
            this._logger = logger;
            this._producer = producer;
            this._snapshots = snapshots;
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
                    case AccountUpdateRequested request:
                        await HandleAccountUpdateRequest(request);
                        break;
                    case AccountUpdateCompensate compensate:
                        await HandleAccountUpdateCompensate(compensate);
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

        /// <summary>
        /// Étape « valider &amp; préparer » du SAGA : vérifie l'identité et applique
        /// l'email, mémorise l'ancien email pour compensation, puis répond à
        /// l'orchestrateur (UserService) via la file de réponse dédiée.
        /// </summary>
        private async Task HandleAccountUpdateRequest(AccountUpdateRequested request)
        {
            var (outcome, oldEmail) = await this._userService.PrepareAccountUpdate(
                request.UserId, request.NewEmail, request.AuthPassword);

            if (outcome == AccountUpdateOutcome.Confirmed && oldEmail != null)
                this._snapshots.Snapshots[request.SagaId] = (request.UserId, oldEmail);

            var reply = new
            {
                SagaId = request.SagaId,
                Status = outcome.ToString(),
                Message = (string?)null
            };

            this._producer.PublishEvent(reply, ReplyQueue);
            this._logger.LogInformation($"Saga {request.SagaId} → {outcome} (User-{request.UserId})");
        }

        /// <summary>
        /// Compensation du SAGA : restaure l'email d'origine si un snapshot existe.
        /// </summary>
        private async Task HandleAccountUpdateCompensate(AccountUpdateCompensate compensate)
        {
            if (this._snapshots.Snapshots.TryRemove(compensate.SagaId, out var snapshot))
            {
                await this._userService.RevertAccountEmail(snapshot.UserId, snapshot.OldEmail);
                this._logger.LogInformation($"Saga {compensate.SagaId} compensated: email restored for User-{snapshot.UserId}");
            }
            else
            {
                this._logger.LogWarning($"Saga {compensate.SagaId} compensation: no snapshot found.");
            }
        }
    }
}
