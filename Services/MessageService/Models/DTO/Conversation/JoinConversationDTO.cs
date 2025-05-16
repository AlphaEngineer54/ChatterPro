using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.DTO.Conversation
{
    public class JoinConversationDTO
    { 
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ConversationId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ConversationId must be a positive integer.")]
        public int ConversationId { get; set; }
    }
}
