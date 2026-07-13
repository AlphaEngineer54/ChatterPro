
using System.ComponentModel.DataAnnotations;

namespace DataExportService.Models
{
    /// <summary>
    /// This class represents a row in the CSV file for conversations.
    /// </summary>
    public class ConversationRowCSV
    {
        // Conversation properties
        public int ConversationId { get; set; }
        public string ConversationTitle { get; set; } = null!;
        public DateTime ConversationDate { get; set; }

        // Message properties (nullable pour gérer les lignes "conversation uniquement")
        public int? MessageId { get; set; }
        public string? MessageContent { get; set; }
        public DateTime? MessageDate { get; set; }
        public int? MessageUserId { get; set; }
        public string? MessageStatus { get; set; }
    }

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
        [RegularExpression("^(read|sent|delivred)$", ErrorMessage = "Le statut doit être 'read', 'sent' ou 'delivred'.")]
        public string Status { get; set; } = null!;

    }

}


