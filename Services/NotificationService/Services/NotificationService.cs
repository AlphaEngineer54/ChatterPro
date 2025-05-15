using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Hubs;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationService
    {
        private readonly NotifDbContext _context;

        private readonly IHubContext<NotificationHubs> _hubContext;

        private readonly ILogger<NotificationService> _logger;

        // Constructor to initialize the NotificationService with the required dependencies
        public NotificationService(NotifDbContext context,
                                   IHubContext<NotificationHubs> hubContext,
                                   ILogger<NotificationService> logger)
        {
            _logger = logger;
            _context = context;
            _hubContext = hubContext;
        }

        // Method to get all notifications for a specific user
        public async Task<IList<Notification>> GetNotifications(int userId)
        {
            var notifications = await _context.Notifications
                            .Where(n => n.UserId == userId)
                            .ToListAsync();

            if (notifications == null || notifications.Count == 0)
            {
                return null;
            }

            return notifications;
        }

        // Create method to add notification into database
        public async Task AddNotification(Notification newNotification)
        {
            // Add the notification to the database
            _context.Notifications.Add(newNotification);
            await _context.SaveChangesAsync();

            // Send the notification to the specific user
            await _hubContext.Clients.User(newNotification.UserId.ToString())
                .SendAsync("ReceiveNotification", newNotification);

            // Add logging to inform that the notification was sent
            // By using ILogger
            _logger.LogInformation($"Notification sent to user {newNotification.UserId}: {newNotification.Message}");
        }

        // Method to delete an notification by userId
        public void DeleteNotification(int userId)
        {
            // Delete the notification from the database
            var notification = _context.Notifications
                .FirstOrDefault(n => n.UserId == userId);

            // Check if the notification exists
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                _context.SaveChanges();

                _logger.LogInformation($"Notification deleted for user {userId}");
            }
            else
            {
                // Log that the notification was not found
                _logger.LogWarning($"Notification not found for user {userId}");
            }
        }

        // Method to delete all notifications for a specific user
        public async Task DeleteAllNotifications(int userId)
        {
            // Delete all notifications for the user
            var notifications = await  _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notifications.Any())
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
