using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Hubs;
using NotificationService.Models;

namespace NotificationService.Services
{
    /// <summary>
    /// NotificationService is responsible for managing notifications for users.
    /// </summary>
    public class NotificationManagerService
    {
        private readonly NotifDbContext _context;

        private readonly IHubContext<NotificationHubs> _hubContext;

        private readonly ILogger<NotificationManagerService> _logger;

        // Constructor to initialize the NotificationService with the required dependencies
        public NotificationManagerService(NotifDbContext context,
                                   IHubContext<NotificationHubs> hubContext,
                                   ILogger<NotificationManagerService> logger)
        {
            _logger = logger;
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Method to get all notifications for a specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IList<Notification>> GetNotifications(int userId)
        {
            var notifications = await _context.Notifications
                            .Where(n => n.UserId == userId)
                            .ToListAsync();

            if (notifications.Count == 0)
            {
                return null;
            }

            return notifications;
        }

        // Method to get a specific notification by its ID
        public async Task<Notification> GetNotification(int notificationId)
        {
            var notification = await _context.Notifications
                            .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                return null;
            }

            return notification;
        }


        // Method to add notification into database for a PRIVATE conversation
        public async Task AddNotification(Notification newNotification)
        {
            // Add the notification to the database
            _context.Notifications.Add(newNotification);
            await _context.SaveChangesAsync();

            // Send the notification to the specific user
            await _hubContext.Clients.User(newNotification.UserId.ToString())
                .SendAsync("ReceiveNotification", newNotification);

            _logger.LogInformation($"Notification sent to user {newNotification.UserId}: {newNotification.Message}");
        }

        // Method to update an notification by userId
        public async Task UpdateNotification(int userId, Notification updatedNotification)
        {
             await _context.SaveChangesAsync();

             _logger.LogInformation($"Notification updated for user {userId}");
        }

        // Method to delete an notification by id
        public async Task DeleteNotification(Notification notification)
        {
            // Check if the notification exists
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Notification-{notification.Id} deleted");
            }
            else
            {
                // Log that the notification was not found
                _logger.LogWarning($"Notification not found");
            }
        }

        // Method to delete all notifications for a specific user
        public async Task DeleteAllNotifications(int userId)
        {
            // Delete all notifications for the user
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notifications.Count != 0)
            {
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"All notifications deleted for user {userId}");
            }
            else
            {
                // Log that no notifications were found
                _logger.LogWarning($"No notifications found for user {userId}");
            }
        }
    }
}
