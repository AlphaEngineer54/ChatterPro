using MessageService.Models;
using MessageService.Models.DTO.Message;
using MessageService.Services;
using Microsoft.AspNetCore.SignalR;

namespace MessageService.Hubs
{
    /// <summary>
    /// SignalR hub for chat functionality.
    /// </summary>
    public class ChatHub : Hub
    {
        private readonly MsgService _messageService;

        public ChatHub(MsgService messageService)
        {
            _messageService = messageService;
        }

        /// <summary>
        /// Handle real-time messaging between clients.
        /// </summary>
        /// <param name="newMessage"></param>
        /// <returns></returns>
        public async Task SendMesage(NewMessageDTO newMessage)
        {
            var message = new Message()
            {
                // Set up properties for the message
                Content = newMessage.Content,
                ReceiverId = newMessage.ReceiverId,
                SenderId = newMessage.SenderId,
                ConversationId = newMessage.ConversationId,
                Date = DateTime.Now,
                Status = newMessage.Status,
            };

            // Save the message to the database
            var createdMessage = await _messageService.CreateMessageAsync(message);
            await Clients.All.SendAsync("ReceiveMessage", createdMessage);
        }

        /// <summary>
        /// Handle real-time messaging to a specific user.
        /// </summary>
        /// <param name="newMessage"></param>
        /// <returns></returns>
        public async Task SendMessageToUser(NewMessageDTO newMessage)
        {

            var message = new Message()
            {
                // Set up properties for the message
                Content = newMessage.Content,
                ReceiverId = newMessage.ReceiverId,
                SenderId = newMessage.SenderId,
                ConversationId = newMessage.ConversationId,
                Date = DateTime.Now,
                Status = newMessage.Status,
            };

            // Save the message to the database
            var createdMessage = await _messageService.CreateMessageAsync(message);

            await Clients.User(createdMessage.ReceiverId.ToString())
                          .SendAsync("ReceiveMessage", createdMessage);
        }
    }
}
