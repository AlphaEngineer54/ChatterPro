using MessageService.Models.DTO.Conversation;
using MessageService.Models;
using Microsoft.AspNetCore.Mvc;
using MessageService.Services;
using MessageService.Interfaces;
using MessageService.Models.DTO.Message;

namespace MessageService.Controllers
{
    /// <summary>
    /// Handles operations related to conversations (CRUD, listing, filtering).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly ConversationService _conversationService;
        private readonly IGenerator<string> _generator;

        public ConversationController(ConversationService conversationService, IGenerator<string> generator)
        {
            _conversationService = conversationService;
            _generator = generator;
        }

        /// <summary>
        /// Creates a new conversation.
        /// </summary>
        /// <param name="dto">Conversation details (title and owner user ID).</param>
        /// <returns>Returns the newly created conversation object.</returns>
        /// <response code="201">Conversation created successfully.</response>
        /// <response code="400">Invalid payload (missing or invalid fields).</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateConversation([FromBody] NewConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conversation = new Conversation
            {
                Title = dto.Title ?? "unknown",
                Date = DateTime.Now,
                JoinCode = _generator.Generate(length: 40),
                OwnerId = dto.UserId
            };

            var created = await _conversationService.CreateConversationAsync(conversation);
            var responseDTO = MapToSimpleDTO(created);

            return CreatedAtAction(nameof(GetConversationById), new { conversationId = responseDTO.Id }, responseDTO);
        }

        /// <summary>
        /// Retrieves all conversations for a given user.
        /// </summary>
        /// <param name="userId">User ID of the conversation owner.</param>
        /// <param name="limit">Maximum number of conversations to return.</param>
        /// <returns>List of user conversations.</returns>
        /// <response code="200">Conversations found.</response>
        /// <response code="404">No conversations found for this user.</response>
        [HttpGet("by-user-id/{userId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllConversationsByUserId(int userId, [FromQuery] int limit)
        {
            var conversations = await _conversationService.GetAllConversationsByUserId(userId, limit);
            if (conversations == null || !conversations.Any())
                return NotFound(new { Message = "No conversations found for this user" });

            var response = conversations.Select(MapToSimpleDTO).ToList();
            return Ok(response);
        }

        /// <summary>
        /// Retrieves all conversations in the system (admin-level access).
        /// </summary>
        /// <param name="limit">Maximum number of conversations to return.</param>
        /// <returns>List of all conversations.</returns>
        /// <response code="200">Conversations retrieved.</response>
        /// <response code="404">No conversations available.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllConversations([FromQuery] int limit)
        {
            var conversations = await _conversationService.GetAllConversationsAsync(limit);
            if (conversations == null || !conversations.Any())
                return NotFound(new { Message = "No conversations found" });

            var response = conversations.Select(MapToSimpleDTO).ToList();
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific conversation by its ID.
        /// </summary>
        /// <param name="conversationId">ID of the conversation.</param>
        /// <param name="limit">Limit the number of messages returned.</param>
        /// <returns>Detailed conversation with messages.</returns>
        /// <response code="200">Conversation found.</response>
        /// <response code="404">Conversation not found.</response>
        [HttpGet("{conversationId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConversationById(int conversationId, [FromQuery] int limit)
        {
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId, limit);
            if (conversation == null)
                return NotFound(new { Message = "Conversation not found" });

            var responseDTO = MapToDetailedDTO(conversation);
            return Ok(responseDTO);
        }

        /// <summary>
        /// Updates an existing conversation's metadata.
        /// </summary>
        /// <param name="conversationId">ID of the conversation to update.</param>
        /// <param name="dto">Updated title and owner ID.</param>
        /// <returns>Updated conversation.</returns>
        /// <response code="200">Conversation updated.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="404">Conversation not found.</response>
        [HttpPut("{conversationId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateConversation(int conversationId, [FromBody] UpdatedConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedDTO = new UpdatedConversationDTO
            {
                Title = dto.Title,
                UserId = dto.UserId
            };

            var conversation = await _conversationService.UpdateConversationAsync(conversationId, updatedDTO);
            if (conversation == null)
                return NotFound(new { Message = "Conversation not found" });

            var responseDTO = MapToSimpleDTO(conversation);
            return Ok(responseDTO);
        }

        /// <summary>
        /// Deletes a conversation by ID.
        /// </summary>
        /// <param name="conversationId">ID of the conversation to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">Conversation successfully deleted.</response>
        /// <response code="404">Conversation not found.</response>
        [HttpDelete("{conversationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteConversation(int conversationId)
        {
            var isDeleted = await _conversationService.DeleteConversationAsync(conversationId);
            return isDeleted
                ? NoContent()
                : NotFound(new { Message = "Conversation not found" });
        }

        /// <summary>
        /// Maps a Conversation entity to a simple DTO.
        /// </summary>
        private ConversationResponseDTO MapToSimpleDTO(Conversation conversation)
        {
            return new ConversationResponseDTO
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Date = conversation.Date,
                JoinCode = conversation.JoinCode,
                OwnerId = conversation.OwnerId
            };
        }

        /// <summary>
        /// Maps a Conversation entity to a detailed DTO including messages.
        /// </summary>
        private ConversationResponseWithMessageDTO MapToDetailedDTO(Conversation conversation)
        {
            return new ConversationResponseWithMessageDTO
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Date = conversation.Date,
                JoinCode = conversation.JoinCode,
                OwnerId = conversation.OwnerId,
                Messages = conversation.Messages?.Select(m => new MessageResponseDTO
                {
                    Id = m.Id,
                    Content = m.Content,
                    Date = m.Date,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Status = m.Status
                }).ToList() ?? new List<MessageResponseDTO>()
            };
        }
    }
}
