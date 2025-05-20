using NotificationService.Events;

namespace NotificationService.Events
{
    public class CreatedMessageEvent : Event
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string? Message { get; set; }
       
        public override string ToString()
        {
            return $"{base.ToString()}, Publisher ID: {SenderId}, Receiver ID: {ReceiverId}, Message: {Message}";
        }
    }
}
