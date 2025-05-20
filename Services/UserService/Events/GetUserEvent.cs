using UserService.Events;

namespace UserService.Events
{
    public class GetUserIEvent : Event
    {
        public int[] ids { get; set; }
    }
}
