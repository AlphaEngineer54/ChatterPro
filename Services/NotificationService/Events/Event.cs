namespace NotificationService.Events
{
    public abstract class Event 
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        // ToString()
        public override string ToString()
        {
            return $"{GetType().Name} - {OccurredOn} - {EventId}";
        }
    }
}
