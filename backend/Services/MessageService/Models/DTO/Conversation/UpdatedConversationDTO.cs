using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.DTO.Conversation
{
    public class UpdatedConversationDTO
    {
        [Required]
        [StringLength(30, ErrorMessage = "Le titre ne peut pas dépassé 30 caractères!")]
        public string? Title { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
