using MessageService.Models.DTO.Message;

namespace MessageService.Models.DTO.Conversation
{
    /// <summary>
    /// Représentation temps réel d'une conversation renvoyée par le hub
    /// (événements JoinedGroup / ConnectedToGroup) : métadonnées, messages
    /// récents et identifiants des participants.
    /// </summary>
    public class ConversationDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime Date { get; set; }
        public string JoinCode { get; set; } = null!;
        public int OwnerId { get; set; }
        public List<MessageResponseDTO> Messages { get; set; } = new();
        public List<int> ParticipantIds { get; set; } = new();
    }
}
