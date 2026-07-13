using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.DTO.Message
{
    public class UpdatedMessageDTO
    {
        [Required]
        [StringLength(250, ErrorMessage = "Le message ne peut pas dépasser 250 caractères!")]
        public string? Content { get; set; }

        [Required]
        [RegularExpression(@"^(sent|delivered|read)$",
                          ErrorMessage = "Status must be one of the following: sent, delivered, read.")]
        public string? Status { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]  
        public int ConversationId { get; set; }
    }
}
