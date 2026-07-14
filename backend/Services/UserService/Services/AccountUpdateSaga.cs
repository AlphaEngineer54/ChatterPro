using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Events;

namespace UserService.Services
{
    /// <summary>
    /// Orchestrateur du SAGA « mise à jour de compte ». Coordonne, en request/reply
    /// sur RabbitMQ, la validation de l'identité et l'application de l'email côté
    /// AuthService (participant), et publie une compensation si le commit local échoue.
    ///
    /// Le contrat HTTP de <c>PUT /user/{id}</c> devant rester synchrone, l'appel
    /// <see cref="RequestAsync"/> bloque jusqu'à la réponse du participant (ou timeout).
    ///
    /// Utilise un canal RabbitMQ dédié (distinct des consommateurs d'événements) et une
    /// file de réponse fixe consommée en continu ; chaque réponse est routée par SagaId
    /// vers la <see cref="TaskCompletionSource{TResult}"/> en attente.
    /// </summary>
    public class AccountUpdateSaga : IDisposable
    {
        private const string RequestQueue = "account-update-requested";
        private const string ReplyQueue = "account-update-reply";
        private const string CompensateQueue = "account-update-compensate";

        private readonly ILogger<AccountUpdateSaga> _logger;
        private readonly IChannel _channel;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<AccountUpdateReply>> _pending = new();

        public AccountUpdateSaga(RabbitMQConnection connection, ILogger<AccountUpdateSaga> logger)
        {
            _logger = logger;

            // Canal dédié à la RPC du SAGA (l'ouvrir tôt = commencer à écouter les réponses).
            _channel = connection.GetConnection().CreateChannelAsync().GetAwaiter().GetResult();

            foreach (var queue in new[] { RequestQueue, ReplyQueue, CompensateQueue })
            {
                _channel.QueueDeclareAsync(queue: queue, durable: false, exclusive: false,
                                           autoDelete: false, arguments: null).GetAwaiter().GetResult();
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var reply = JsonConvert.DeserializeObject<AccountUpdateReply>(json);
                    if (reply != null && _pending.TryRemove(reply.SagaId, out var tcs))
                        tcs.TrySetResult(reply);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to process saga reply: {ex.Message}");
                }

                return Task.CompletedTask;
            };

            _channel.BasicConsumeAsync(queue: ReplyQueue, autoAck: true, consumer: consumer)
                    .GetAwaiter().GetResult();

            _logger.LogInformation("AccountUpdateSaga orchestrator ready (listening on account-update-reply).");
        }

        /// <summary>
        /// Publie la commande de validation et attend la réponse du participant.
        /// Renvoie une réponse de statut <c>Error</c> en cas de timeout.
        /// </summary>
        public async Task<AccountUpdateReply> RequestAsync(AccountUpdateRequested command, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<AccountUpdateReply>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pending[command.SagaId] = tcs;

            try
            {
                await PublishAsync(RequestQueue, command);

                using var cts = new CancellationTokenSource(timeout);
                await using (cts.Token.Register(() =>
                {
                    if (_pending.TryRemove(command.SagaId, out var pending))
                    {
                        pending.TrySetResult(new AccountUpdateReply
                        {
                            SagaId = command.SagaId,
                            Status = nameof(AccountUpdateStatus.Error),
                            Message = "Délai dépassé en attendant AuthService."
                        });
                    }
                }))
                {
                    return await tcs.Task;
                }
            }
            finally
            {
                _pending.TryRemove(command.SagaId, out _);
            }
        }

        /// <summary>Publie la commande de compensation (best-effort).</summary>
        public Task PublishCompensateAsync(Guid sagaId)
            => PublishAsync(CompensateQueue, new AccountUpdateCompensate { SagaId = sagaId });

        private async Task PublishAsync<T>(string queue, T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _channel.BasicPublishAsync(exchange: "", routingKey: queue, body: body);
        }

        public void Dispose() => _channel?.CloseAsync();
    }
}
