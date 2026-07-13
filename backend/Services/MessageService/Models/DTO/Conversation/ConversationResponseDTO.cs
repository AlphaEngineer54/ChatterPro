namespace MessageService.Models.DTO.Conversation
{
    public class ConversationResponseDTO
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string JoinCode { get; set; }
    }
}
