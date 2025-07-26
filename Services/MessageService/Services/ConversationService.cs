using MessageService.Events;
using MessageService.Interfaces;
using MessageService.Models;
using MessageService.Models.DTO.Conversation;
using Microsoft.EntityFrameworkCore;

namespace MessageService.Services
{
    public class ConversationService
    {
        private readonly MessageDbContext _context;
        private readonly IProducer _producer;

        public ConversationService(MessageDbContext context, IProducer producer)
        {
            this._context = context;
            this._producer = producer;
        }

        // CREATE: Ajouter une nouvelle conversation
        public async Task<Conversation> CreateConversationAsync(Conversation newConversation)
        {
            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

           var userConversationJoin = new UserConversation()
           {
              ConversationId = newConversation.Id,
              UserId = newConversation.OwnerId
           };

            // Ajouter un enregistrement à la table de jointure (MANY-TO-MANY)
             _context.UserConversations.Add(userConversationJoin);

            await _context.SaveChangesAsync();
            return newConversation;
        }

        // READ: Récupérer toutes les conversations
        public async Task<IEnumerable<Conversation>> GetAllConversationsAsync()
        {
            return await _context.Conversations.ToListAsync();
        }

        // READ: Récupérer toutes les conversations d'un utilisateur par son identifiant
        public async Task<IEnumerable<Conversation>> GetAllConversationsByUserId(int userId)
        {
            var conversationIds = await _context.UserConversations
                                                .Where(uc => uc.UserId == userId)
                                                .Select(uc => uc.ConversationId)
                                                .ToListAsync();

            var conversations = await _context.Conversations
                                              .Where(c => conversationIds.Contains(c.Id))
                                              .ToListAsync();

            return conversations;
        }


        // READ: Récupérer une conversation par son identifiant
        public async Task<Conversation> GetConversationByIdAsync(int conversationId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

           var userConversations = await this._context.UserConversations
                                        .Where(uc => uc.ConversationId == conversationId)
                                        .ToListAsync();

            var ids = userConversations.Select(uc => uc.UserId).ToArray();

            // Envoyer l'event pour fetch les infos des utilisateurs d'une conversation
            this._producer.Send(new GetUserIEvent() {UserId = conversation.OwnerId, ids = ids }, "get-user-event");
           
            return conversation;
        }

        // UPDATE : Add new user in the conversation via joinCode
        public async Task<Conversation?> AddUserToConversationAsync(string joinCode, int userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Users) // Inclure les relations pour éviter les problèmes d'accès
                .FirstOrDefaultAsync(c => c.JoinCode == joinCode);

            if (conversation == null)
            {
                // Retourner null ou lever une exception pour mieux gérer l'absence
                return null;
            }

            // Vérifier si l'utilisateur n'est pas déjà dans la conversation
            if (conversation.Users.Any(uc => uc.UserId == userId))
            {
                return conversation; // L'utilisateur est déjà dans la conversation
            }

            var userConversationJoin = new UserConversation()
            {
                ConversationId = conversation.Id,
                UserId = userId,
            };

            this._context.UserConversations.Add(userConversationJoin);
            await _context.SaveChangesAsync();
            return conversation;
        }

        // UPDATE: Modifier une conversation existante
        public async Task<Conversation?> UpdateConversationAsync(int conversationId, UpdatedConversationDTO updatedConversation)
        {
            var existingConversation = await _context.Conversations
                .Include(c => c.Users) // Inclure les relations pour éviter les problèmes d'accès
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (existingConversation == null)
            {
                // Retourner null ou lever une exception pour mieux gérer l'absence
                return null;
            }

            // Mettre à jour les propriétés de la conversation
            existingConversation.Title = updatedConversation.Title;

            var userConversationJoin = new UserConversation()
            {
                ConversationId = existingConversation.Id,
                UserId = updatedConversation.UserId,
            };

            this._context.UserConversations.Add(userConversationJoin);
            await _context.SaveChangesAsync();
            return existingConversation;
        }

        // UPDATE : Modifier une conversation sans DTO
        public async Task UpdateConversationWithoutDTOAsync(Conversation updatedConversation)
        { 
            await _context.SaveChangesAsync();
        }

        // DELETE: Supprimer une conversation
        public async Task<bool> DeleteConversationAsync(int conversationId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);
            
            if (conversation == null)
            {
                return false; // Conversation non trouvée
            }

            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
