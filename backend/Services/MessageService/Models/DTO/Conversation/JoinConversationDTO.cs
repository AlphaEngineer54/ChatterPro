using System.ComponentModel.DataAnnotations;

namespace MessageService.Models.DTO.Conversation
{
    public class JoinConversationDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "JoinCode is required.")]
        [RegularExpression(@"^([A-Z0-9]{4}-){0,8}[A-Z0-9]{0,4}$", ErrorMessage = "JoinCode must be alphanumeric groups separated by dashes.")]
        [StringLength(40, ErrorMessage = "JoinCode cannot exceed 40 characters.")]
        public string JoinCode { get; set; } = string.Empty;
    }
}
