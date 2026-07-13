using AuthService.Events;

namespace AuthService.Interfaces
{
    public interface IEventHandler
    {
        Task HandleEventAsync(Event eventMessage);
    }
}
