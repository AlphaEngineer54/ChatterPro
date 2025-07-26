using MessageService.Models.DTO.Conversation;
using MessageService.Models;
using MessageService.Services;
using Microsoft.AspNetCore.Mvc;
using MessageService.Interfaces;
using MessageService.Models.DTO.Message;

namespace MessageService.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] NewConversationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conversation = new Conversation
            {
                Title = dto.Title ?? "unknown",
                Date = DateTime.Now,
                JoinCode = _generator.Generate(40),
                OwnerId = dto.UserId
            };

            var created = await _conversationService.CreateConversationAsync(conversation);
            var responseDTO = MapToSimpleDTO(created);

            return CreatedAtAction(nameof(GetConversationById), new { conversationId = responseDTO.Id }, responseDTO);
        }

        [HttpGet("by-user-id/{userId}")]
        public async Task<IActionResult> GetAllConversationsByUserId(int userId)
        {
            var conversations = await _conversationService.GetAllConversationsByUserId(userId);
            if (conversations == null || !conversations.Any())
                return NotFound(new { Message = "No conversations found for this user" });

            var response = conversations.Select(MapToSimpleDTO).ToList();
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConversations()
        {
            var conversations = await _conversationService.GetAllConversationsAsync();
            if (conversations == null || !conversations.Any())
                return NotFound(new { Message = "No conversations found" });

            var response = conversations.Select(MapToSimpleDTO).ToList();
            return Ok(response);
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversationById(int conversationId)
        {
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);
            if (conversation == null)
                return NotFound(new { Message = "Conversation not found" });

            var responseDTO = MapToDetailedDTO(conversation);
            return Ok(responseDTO);
        }

        [HttpPut("{conversationId}")]
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

        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> DeleteConversation(int conversationId)
        {
            var isDeleted = await _conversationService.DeleteConversationAsync(conversationId);
            return isDeleted
                ? NoContent()
                : NotFound(new { Message = "Conversation not found" });
        }

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
