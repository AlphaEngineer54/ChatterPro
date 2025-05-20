using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.DTO
{
    public class NotificationDTO
    {
        [Required(ErrorMessage = "Please provide an valid message")]
        [StringLength(255, ErrorMessage = "Message length cannot exceed 255 characters.")]
        public string Message { get; set; } = null!;

        [Required(ErrorMessage = "Please provide a user ID")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int UserId { get; set; }
    }
}
