using UserService.Events;

namespace UserService.Interfaces
{
    public interface IEventHandler
    {
        Task HandleEventAsync(Event eventMessage);
    }

}
