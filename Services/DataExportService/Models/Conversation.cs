
using System.ComponentModel.DataAnnotations;

namespace DataExportService.Models
{
    // Modèle de donnée à exporter
    public partial class Conversation
    {
        [Required(ErrorMessage = "Please enter a valid id for an existing conversation")]
        public int Id { get; set; } 

        [Required]
        [StringLength(50)]
        public string Title { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public virtual IList<Message> Messages { get; set; } = new List<Message>();
    }

    // Modèle de donnée à exporter
    public partial class Message
    {
        [Required]
        public int Id { get; set; } 

        [Required]
        [StringLength(maximumLength: 250)]
        public string Content { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(5)]
        [RegularExpression("^(read|sent|delivred)$", ErrorMessage = "Le statut doit être 'read', 'sent' ou 'delivred'.")]
        public string Status { get; set; } = null!;

    }

}


