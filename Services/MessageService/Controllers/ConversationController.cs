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
        public async Task<ActionResult<Conversation>> CreateConversation([FromBody] NewConversationDTO newConversationDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Mapper le DTO vers l'entité Conversation
            var newConversation = new Conversation
            {
                Title = newConversationDTO.Title,
                Date = newConversationDTO.Date ?? DateTime.Now,
                
            };

            var createdConversation = await _conversationService.CreateConversationAsync(newConversationDTO.UserId, newConversation);

            return CreatedAtAction(nameof(GetConversationById), new { conversationId = createdConversation.Id }, createdConversation);
        }

        // READ: Récupérer toutes les conversations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conversation>>> GetAllConversations()
        {
            var conversations = await _conversationService.GetAllConversationsAsync();

            if (conversations == null || !conversations.Any())
            {
                return NotFound(new { Message = "No conversations found" });
            }

            return Ok(conversations);
        }

        // READ: Récupérer une conversation par son identifiant
        [HttpGet("{conversationId}")]
        public async Task<ActionResult<Conversation>> GetConversationById(int conversationId)
        {
            var conversation = await _conversationService.GetConversationByIdAsync(conversationId);

            if (conversation == null)
            {
                return NotFound(new { Message = "Conversation not found" });
            }

            return Ok(conversation);
        }

   
        // UPDATE: Modifier une conversation existante
        [HttpPut("{conversationId}")]
        public async Task<ActionResult<Conversation>> UpdateConversation(int conversationId, [FromBody] UpdatedConversationDTO updatedConversationDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Mapper le DTO vers l'entité Conversation
            var updatedConversation = new UpdatedConversationDTO
            {
                Title = updatedConversationDTO.Title,
                UserId = updatedConversationDTO.UserId,
            };

            var conversation = await _conversationService.UpdateConversationAsync(conversationId, updatedConversation);

            if (conversation == null)
            {
                return NotFound(new { Message = "Conversation not found" });
            }

            return Ok(conversation);
        }

        // DELETE: Supprimer une conversation
        [HttpDelete("{conversationId}")]
        public async Task<ActionResult> DeleteConversation(int conversationId)
        {
            var isDeleted = await _conversationService.DeleteConversationAsync(conversationId);

            if (!isDeleted)
            {
                return NotFound(new { Message = "Conversation not found" });
            }

            return NoContent(); // 204 No Content - Suppression réussie
        }
    }

}
