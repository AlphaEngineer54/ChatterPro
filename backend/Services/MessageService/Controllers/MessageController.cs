using Microsoft.AspNetCore.Mvc;
using MessageService.Models;
using MessageService.Services;
using MessageService.Models.DTO.Message;

namespace MessageService.Controllers
{
    /// <summary>
    /// Manages CRUD operations for messages within conversations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MsgService _messageService;

        public MessageController(MsgService messageService)
        {
            _messageService = messageService;
        }

        /// <summary>
        /// Creates a new message in a conversation.
        /// </summary>
        /// <param name="newMessage">The message data to create.</param>
        /// <returns>Returns the created message details.</returns>
        /// <response code="201">Message created successfully.</response>
        /// <response code="400">Invalid message payload.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Retrieves all messages.
        /// </summary>
        /// <returns>List of all messages.</returns>
        /// <response code="200">Messages retrieved successfully.</response>
        /// <response code="404">No messages found.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _messageService.GetAllMessagesAsync();
            if (messages == null || !messages.Any())
                return NotFound(new { Message = "No messages found" });

            var response = messages.Select(MapToDTO).ToList();
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific message by ID.
        /// </summary>
        /// <param name="messageId">The ID of the message to retrieve.</param>
        /// <returns>The message details.</returns>
        /// <response code="200">Message found.</response>
        /// <response code="404">Message not found.</response>
        [HttpGet("{messageId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMessageById(int messageId)
        {
            var message = await _messageService.GetMessageByIdAsync(messageId);
            if (message == null)
                return NotFound(new { Message = "Message not found" });

            var responseDTO = MapToDTO(message);
            return Ok(responseDTO);
        }

        /// <summary>
        /// Updates an existing message.
        /// </summary>
        /// <param name="messageId">The ID of the message to update.</param>
        /// <param name="updatedMessage">The updated message data.</param>
        /// <returns>The updated message details.</returns>
        /// <response code="200">Message updated successfully.</response>
        /// <response code="400">Invalid update payload.</response>
        /// <response code="404">Message not found.</response>
        [HttpPut("{messageId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Deletes a message by its ID.
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">Message deleted successfully.</response>
        /// <response code="404">Message not found.</response>
        [HttpDelete("{messageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var isDeleted = await _messageService.DeleteMessageAsync(messageId);
            return isDeleted
                ? NoContent()
                : NotFound(new { Message = "Message not found" });
        }

        /// <summary>
        /// Maps a Message entity to a MessageResponseDTO.
        /// </summary>
        /// <param name="message">The message entity.</param>
        /// <returns>The mapped DTO.</returns>
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
    }
}
