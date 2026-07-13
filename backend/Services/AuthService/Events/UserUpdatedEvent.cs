namespace AuthService.Events
{
    public class UserUpdatedEvent : Event
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        public override string ToString()
        {
            return $"[UserCreatedEvent - UserID: {Id}]";
        }
    }
}
