using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTO.User
{
    public class UserUpdateDTO
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [StringLength(50)]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        public string? Password { get; set; }
        
    }
}
