namespace UserService.Events
{
    public class UserDeletedEvent : Event
    {
        public int Id { get; set; }
    }
}
