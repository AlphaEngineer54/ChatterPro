using MessageService.Models;
using MessageService.Models.DTO.Conversation;
using MessageService.Models.DTO.Message;
using MessageService.Services;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;

namespace MessageService.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MsgService _messageService;
        private readonly ConversationService _conversationService;

        public ChatHub(MsgService messageService, ConversationService conversationService)
        {
            _messageService = messageService;
            _conversationService = conversationService;
        }
        
        public async Task SendMessage(NewMessageDTO newMessage)
        {
            if (await ValidateAndRejectIfInvalid(newMessage)) return;

            var createdMessage = await CreateAndPersistMessageAsync(newMessage);
            await Clients.All.SendAsync("ReceiveMessage", createdMessage);
        }

        public async Task SendMessageToUser(NewMessageDTO newMessage)
        {
            if (await ValidateAndRejectIfInvalid(newMessage)) return;

            var createdMessage = await CreateAndPersistMessageAsync(newMessage);
            await Clients.User(createdMessage.ReceiverId.ToString())
                         .SendAsync("ReceiveMessage", createdMessage);
        }

        public async Task JoinGroup(JoinConversationDTO newUser)
        {
            if (await ValidateAndRejectIfInvalid(newUser)) return;

            var conversation = await _conversationService.AddUserToConversationAsync(newUser.JoinCode, newUser.UserId);
            if (conversation == null)
            {
                await Clients.Caller.SendAsync("Error", "Failed to join conversation. Invalid JoinCode or UserId.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id.ToString());

            var dto = MapConversationToDto(conversation);
            await Clients.Caller.SendAsync("JoinedGroup", dto);
        }

        public async Task ConnectToGroup(int conversationId)
        {
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId, 25);
            if (conversation == null)
            {
                await Clients.Caller.SendAsync("Error", "Conversation not found.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());

            var dto = MapConversationToDto(conversation);
            await Clients.Caller.SendAsync("ConnectedToGroup", dto);
        }

        public async Task SendMessageToGroup(NewMessageDTO newMessage)
        {
            if (await ValidateAndRejectIfInvalid(newMessage)) return;

            var createdMessage = await CreateAndPersistMessageAsync(newMessage);
            await Clients.Group(newMessage.ConversationId.ToString())
                         .SendAsync("ReceiveMessage", createdMessage);
        }

        private async Task<bool> ValidateAndRejectIfInvalid(object model)
        {
            IList<string> errors = ValidateModel(model);
            if (errors.Count > 0)
            {
                await Clients.Caller.SendAsync("ValidationError", errors);
                return true;
            }
            return false;
        }

        private IList<string> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);

            return validationResults.Select(vr => vr.ErrorMessage!).ToList();
        }

        private async Task<MessageResponseDTO> CreateAndPersistMessageAsync(NewMessageDTO dto)
        {
            var message = new Message
            {
                Content = dto.Content,
                ReceiverId = dto.ReceiverId,
                SenderId = dto.SenderId,
                ConversationId = dto.ConversationId,
                Date = DateTime.Now,
                Status = dto.Status
            };

            var createdMessage = await _messageService.CreateMessageAsync(message);

            return new MessageResponseDTO
            {
                Id = createdMessage.Id,
                Content = createdMessage.Content,
                Date = createdMessage.Date,
                SenderId = createdMessage.SenderId,
                ReceiverId = createdMessage.ReceiverId,
                Status = createdMessage.Status
            };
        }

        private ConversationDTO MapConversationToDto(Conversation conversation)
        {
            return new ConversationDTO
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Date = conversation.Date,
                JoinCode = conversation.JoinCode,
                OwnerId = conversation.OwnerId,
                // Les navigations ne sont pas toujours chargées selon l'appelant
                // (JoinGroup charge Users, GetConversationById charge Messages) → null-safe.
                Messages = (conversation.Messages ?? new List<Message>())
                    .OrderBy(m => m.Date)
                    .Select(m => new MessageResponseDTO
                    {
                        Id = m.Id,
                        Content = m.Content,
                        Date = m.Date,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        Status = m.Status
                    }).ToList(),
                ParticipantIds = (conversation.Users ?? new List<UserConversation>())
                    .Select(u => u.UserId)
                    .ToList()
            };
        }
    }
}
