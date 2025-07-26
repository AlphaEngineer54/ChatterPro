using Microsoft.AspNetCore.Mvc;
using MessageService.Services;
using MessageService.Models;
using MessageService.Models.DTO.Message;

namespace MessageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MsgService _messageService;

        public MessageController(MsgService messageService)
        {
            _messageService = messageService;
        }

        // ✨ Méthode de mapping DTO
        private MessageResponseDTO MapToDTO(Message message)
        {
            return new MessageResponseDTO
            {
                Id = message.Id,
                Content = message.Content,
                Date = message.Date,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Status = message.Status
            };
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] NewMessageDTO newMessage)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = new Message
            {
                Content = newMessage.Content,
                ReceiverId = newMessage.ReceiverId,
                SenderId = newMessage.SenderId,
                ConversationId = newMessage.ConversationId,
                Date = DateTime.Now,
                Status = newMessage.Status
            };

            var createdMessage = await _messageService.CreateMessageAsync(message);
            var responseDTO = MapToDTO(createdMessage);

            return CreatedAtAction(nameof(GetMessageById), new { messageId = responseDTO.Id }, responseDTO);
        }

        // READ: tous les messages
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _messageService.GetAllMessagesAsync();
            if (messages == null || !messages.Any())
                return NotFound(new { Message = "No messages found" });

            var response = messages.Select(MapToDTO).ToList();
            return Ok(response);
        }

        // READ: message par ID
        [HttpGet("{messageId}")]
        public async Task<IActionResult> GetMessageById(int messageId)
        {
            var message = await _messageService.GetMessageByIdAsync(messageId);
            if (message == null)
                return NotFound(new { Message = "Message not found" });

            var responseDTO = MapToDTO(message);
            return Ok(responseDTO);
        }

        // UPDATE
        [HttpPut("{messageId}")]
        public async Task<IActionResult> UpdateMessage(int messageId, [FromBody] UpdatedMessageDTO updatedMessage)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var messageToUpdate = new Message
            {
                Id = messageId,
                Content = updatedMessage.Content,
                SenderId = updatedMessage.SenderId,
                ConversationId = updatedMessage.ConversationId,
                Status = updatedMessage.Status,
                Date = DateTime.Now
            };

            var updatedMessageDB = await _messageService.UpdateMessageAsync(messageId, messageToUpdate);
            if (updatedMessageDB == null)
                return NotFound(new { Message = "Message not found" });

            var responseDTO = MapToDTO(updatedMessageDB);
            return Ok(responseDTO);
        }

        // DELETE
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var isDeleted = await _messageService.DeleteMessageAsync(messageId);
            return isDeleted
                ? NoContent()
                : NotFound(new { Message = "Message not found" });
        }
    }
}
