using MessageService.Models.DTO.Message;

namespace MessageService.Models.DTO.Conversation
{
    public class ConversationResponseWithMessageDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string JoinCode { get; set; }
        public int OwnerId { get; set; }

        public List<MessageResponseDTO> Messages { get; set; }
    }
}
