using MessageService.Events;
using MessageService.Interfaces;
using MessageService.Models;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Services
{
    public class MsgService
    {
        private readonly MessageDbContext _dbContext;
        private readonly IProducer _producer;

        public MsgService(MessageDbContext dbContext, IProducer producer)
        {
            this._dbContext = dbContext;
            this._producer = producer;
        }

        // CREATE: Ajouter un nouveau message
        public async Task<Message> CreateMessageAsync(Message newMessage)
        {
            // Ajouter le message dans la base de données
            _dbContext.Messages.Add(newMessage);
            await _dbContext.SaveChangesAsync();

            // Publier une notification par destinataire (voir NotifyRecipientsAsync)
            await NotifyRecipientsAsync(newMessage);

            // Retourner le message au client
            return newMessage;
        }

        /// <summary>
        /// Publie un événement de notification par destinataire réel du message.
        /// En chat de groupe, le ReceiverId vaut 0 (placeholder) : on cible donc tous
        /// les membres de la conversation sauf l'expéditeur, chacun avec son vrai UserId
        /// (sinon la notification serait créée pour l'utilisateur 0 et jamais affichée).
        /// </summary>
        private async Task NotifyRecipientsAsync(Message message)
        {
            var recipientIds = await _dbContext.UserConversations
                .Where(uc => uc.ConversationId == message.ConversationId && uc.UserId != message.SenderId)
                .Select(uc => uc.UserId)
                .Distinct()
                .ToListAsync();

            // Repli : destinataire explicite (messagerie 1-à-1) si aucun membre trouvé.
            if (recipientIds.Count == 0 && message.ReceiverId > 0)
            {
                recipientIds.Add(message.ReceiverId);
            }

            foreach (var recipientId in recipientIds)
            {
                _producer.Send(new CreatedMessageEvent
                {
                    Message = message.Content,
                    SenderId = message.SenderId,
                    ReceiverId = recipientId,
                }, "new-message-event");
            }
        }

        // READ: Récupérer tous les messages
        public async Task<IEnumerable<Message>> GetAllMessagesAsync()
        {
            return await _dbContext.Messages.ToListAsync();
        }

        // READ: Récupérer un message par son identifiant
        public async Task<Message> GetMessageByIdAsync(int messageId)
        {
            return await _dbContext.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        // UPDATE: Modifier un message existant
        public async Task<Message> UpdateMessageAsync(int messageId, Message updatedMessage)
        {
            var existingMessage = await _dbContext.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (existingMessage == null)
            {
                return null; // Message non trouvé
            }

            // Mettre à jour les propriétés du message existant
            existingMessage.Content = updatedMessage.Content;
            existingMessage.Status = updatedMessage.Status;
            existingMessage.ConversationId = updatedMessage.ConversationId;

            await _dbContext.SaveChangesAsync();
            return existingMessage;
        }

        // DELETE: Supprimer un message
        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            var message = await _dbContext.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                return false; // Message non trouvé
            }

            _dbContext.Messages.Remove(message);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
