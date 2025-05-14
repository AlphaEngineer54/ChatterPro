namespace AuthService.Events
{
    public class UserDeletedEvent : Event
    {
        public int Id { get; set; }
    }
}
