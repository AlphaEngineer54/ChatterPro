namespace UserService.Events
{
    public class UserCreatedEvent : Event   
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
  
        public override string ToString()
        {
            return $"[UserCreatedEvent - UserID: {Id}, UserName: {UserName}";
        }
    }
}
