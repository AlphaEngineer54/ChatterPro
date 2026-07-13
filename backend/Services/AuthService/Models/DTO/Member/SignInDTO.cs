using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class SignInDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string? Password { get; set; }
    }
}
