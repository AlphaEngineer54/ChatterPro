namespace MessageService.Events
{
    public class GetUserIEvent : Event
    {
        public int UserId { get; set; } // The ID of the user to retrieve
        public int[] ids {  get; set; }
    }
}
