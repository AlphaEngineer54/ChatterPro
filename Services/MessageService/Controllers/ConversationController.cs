using MessageService.Models.DTO.Conversation;
using MessageService.Models;
using MessageService.Services;
using Microsoft.AspNetCore.Mvc;
namespace MessageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly ConversationService _conversationService;

        public ConversationController(ConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        // CREATE: Ajouter une nouvelle conversation
        [HttpPost]
        public async Task<ActionResult> CreateConversation([FromBody] NewConversationDTO newConversationDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newConversation = new Conversation
            {
                Title = newConversationDTO.Title,
                Date = newConversationDTO.Date ?? DateTime.Now
            };

            var createdConversation = await _conversationService.CreateConversationAsync(newConversationDTO.UserId, newConversation);

            return CreatedAtAction(nameof(GetConversationById), new { conversationId = createdConversation.Id }, new
            {
                Id = createdConversation.Id,
                Title = createdConversation.Title,
                Date = createdConversation.Date,
                UserId = newConversationDTO.UserId,
                Messages = createdConversation.Messages
            });
        }

        // GET: Récupérer toutes les conversations d'un utilisateur
        [HttpGet("by-user-id/{userId}")]
        public async Task<ActionResult> GetAllConversationsByUserId(int userId)
        {
            var conversations = await _conversationService.GetAllConversationsByUserId(userId);

            if (conversations == null || !conversations.Any())
                return NotFound(new { Message = "No conversations found for this user" });

            var response = conversations.Select(c => new
            {
                Id = c.Id,
                Title = c.Title,
                Date = c.Date,
                UserId = userId,
                Messages = c.Messages
            });

            return Ok(response);
        }

        // READ: Récupérer toutes les conversations
        [HttpGet]
        public async Task<ActionResult> GetAllConversations()
        {
            var conversations = await _conversationService.GetAllConversationsAsync();

            if (conversations == null || !conversations.Any())
                return NotFound(new { Message = "No conversations found" });

            var response = conversations.Select(c => new
            {
                Id = c.Id,
                Title = c.Title,
                Date = c.Date,
                User = c.Users,
                Messages = c.Messages
            });

            return Ok(response);
        }

        // READ: Récupérer une conversation par son identifiant
        [HttpGet("{conversationId}")]
        public async Task<ActionResult> GetConversationById(int conversationId)
        {
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);

            if (conversation == null)
                return NotFound(new { Message = "Conversation not found" });

            return Ok(new
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Date = conversation.Date,
                UserId = conversation.Users.Select(u => u.UserId),
                Messages = conversation.Messages
            });
        }

        // UPDATE: Modifier une conversation existante
        [HttpPut("{conversationId}")]
        public async Task<ActionResult> UpdateConversation(int conversationId, [FromBody] UpdatedConversationDTO updatedConversationDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedDTO = new UpdatedConversationDTO
            {
                Title = updatedConversationDTO.Title,
                UserId = updatedConversationDTO.UserId
            };

            var conversation = await _conversationService.UpdateConversationAsync(conversationId, updatedDTO);

            if (conversation == null)
                return NotFound(new { Message = "Conversation not found" });

            return Ok(new
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Date = conversation.Date,
                UserId = updatedConversationDTO.UserId,
                Messages = conversation.Messages
            });
        }

        // DELETE: Supprimer une conversation
        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> DeleteConversation(int conversationId)
        {
            var isDeleted = await _conversationService.DeleteConversationAsync(conversationId);

            return isDeleted
                ? NoContent()
                : NotFound(new { Message = "Conversation not found" });
        }
    }
}
