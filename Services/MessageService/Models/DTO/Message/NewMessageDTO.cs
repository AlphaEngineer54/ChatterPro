using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.DTO.Message
{
    public class NewMessageDTO
    {
        [Required]
        [StringLength(250, ErrorMessage = "Le message ne peut pas dépasser 250 caractères!")]
        public string? Content { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string? Status { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ConversationId { get; set; }
    }
}
