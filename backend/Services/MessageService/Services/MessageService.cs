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

            // Créer l'événement de message
            var messageEvent = new CreatedMessageEvent()
            {
                Message = newMessage.Content,
                ReceiverId = newMessage.ReceiverId,
                SenderId = newMessage.SenderId,
            };

            // Publier le message dans RabbitMQ
            _producer.Send(messageEvent, "new-message-event");
            
            // Retourner le message au client
            return newMessage;
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
