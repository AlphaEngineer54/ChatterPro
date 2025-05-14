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

        // CREATE: Ajouter un nouveau message
        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] NewMessageDTO newMessage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var message = new Message()
            {
                Content = newMessage.Content,
                UserId = newMessage.UserId,
                ConversationId = newMessage.ConversationId, 
                Date = DateTime.Now,
                Status = newMessage.Status,
            };

            var createdMessage = await _messageService.CreateMessageAsync(message);

            return CreatedAtAction(nameof(GetMessageById), new { messageId = createdMessage.Id }, createdMessage);
        }

        // READ: Récupérer tous les messages
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _messageService.GetAllMessagesAsync();

            if (messages == null || !messages.Any())
            {
                return NotFound(new { Message = "No messages found" });
            }

            return Ok(messages);
        }

        // READ: Récupérer un message par son identifiant
        [HttpGet("{messageId}")]
        public async Task<IActionResult> GetMessageById(int messageId)
        {
            var message = await _messageService.GetMessageByIdAsync(messageId);

            if (message == null)
            {
                return NotFound(new { Message = "Message not found" });
            }

            return Ok(message);
        }

        // UPDATE: Modifier un message existant
        [HttpPut("{messageId}")]
        public async Task<IActionResult> UpdateMessage(int messageId, [FromBody] UpdatedMessageDTO updatedMessage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var message = new Message()
            {
                Id = messageId,
                Content = updatedMessage.Content,
                UserId = updatedMessage.UserId,
                ConversationId = updatedMessage.ConversationId,
                Status = updatedMessage.Status,
            };

            var updatedMessageDB = await _messageService.UpdateMessageAsync(messageId, message);

            if (updatedMessageDB == null)
            {
                return NotFound(new { Message = "Message not found" });
            }

            return Ok(message);
        }

        // DELETE: Supprimer un message
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var isDeleted = await _messageService.DeleteMessageAsync(messageId);

            if (!isDeleted)
            {
                return NotFound(new { Message = "Message not found" });
            }

            return NoContent(); // 204 No Content - Suppression réussie
        }
    }

}
