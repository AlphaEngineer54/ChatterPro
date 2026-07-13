namespace MessageService.Models.DTO.Message
{
    public class MessageResponseDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime Date { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Status { get; set; } = null!;
    }
}
