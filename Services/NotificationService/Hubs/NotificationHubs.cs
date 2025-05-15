using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Hubs
{
    public class NotificationHubs : Hub
    {
        public async Task SendNotification(Notification newNotification)
        {
            // Send the notification to the specific user
            await Clients.User(newNotification.UserId.ToString()).SendAsync("ReceiveNotification", newNotification);
        }
    }
}
