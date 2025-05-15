using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Models.DTO;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        // Inject notificationService and add some crud method to managing notifications
        private readonly NotificationManagerService _notificationService;

        public NotificationsController(NotificationManagerService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var notifications = await _notificationService.GetNotifications(userId);

            if (!notifications.Any())
            {
                return NotFound(new { Message = "No notifications found." });
            }

            return Ok(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> AddNotification([FromBody] NotificationDTO newNotification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = new Notification()
            {
                Message = newNotification.Message,
                CreatedAt = DateTime.Now,
                UserId = newNotification.UserId
            };

            await _notificationService.AddNotification(notification);

            return CreatedAtAction(nameof(GetNotifications), new { userId = newNotification.UserId }, newNotification);
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var notification = await _notificationService.GetNotification(notificationId);

            if (notification == null)
            {
                return NotFound(new { Message = "Notification not found." });
            }

            await _notificationService.DeleteNotification(notificationId);

            return NoContent();
        }

        [HttpPut("{notificationId}")]
        public async Task<IActionResult> UpdateNotification(int notificationId, [FromBody] NotificationDTO updatedNotification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _notificationService.GetNotification(notificationId);

            if (notification == null)
            {
                return NotFound(new { Message = "Notification not found." });
            }

            await _notificationService.UpdateNotification(notificationId, updatedNotification);

            return NoContent();
        }
    }
}
