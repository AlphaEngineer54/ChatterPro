using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Models.DTO;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    /// <summary>
    /// Manages user notifications (retrieve, create, update, delete).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationManagerService _notificationService;

        public NotificationsController(NotificationManagerService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Retrieves all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications to fetch.</param>
        /// <returns>List of notifications.</returns>
        /// <response code="200">Notifications found and returned.</response>
        /// <response code="404">No notifications exist for the specified user.</response>
        [HttpGet("{userId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NotificationDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var notifications = await _notificationService.GetNotifications(userId);
            if (notifications == null || !notifications.Any())
                return NotFound(new { Message = "No notifications found." });

            return Ok(notifications);
        }

        /// <summary>
        /// Adds a new notification for a user.
        /// </summary>
        /// <param name="newNotification">Notification payload (message and target user).</param>
        /// <returns>The created notification DTO.</returns>
        /// <response code="201">Notification created successfully.</response>
        /// <response code="400">Invalid notification payload.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(NotificationDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddNotification([FromBody] NotificationDTO newNotification)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = new Notification
            {
                Message = newNotification.Message,
                CreatedAt = DateTime.Now,
                UserId = newNotification.UserId
            };

            await _notificationService.AddNotification(notification);

            return CreatedAtAction(
                nameof(GetNotifications),
                new { userId = newNotification.UserId },
                newNotification);
        }

        /// <summary>
        /// Deletes a single notification by its ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to delete.</param>
        /// <response code="204">Notification deleted.</response>
        /// <response code="404">Notification not found.</response>
        [HttpDelete("{notificationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var notification = await _notificationService.GetNotification(notificationId);
            if (notification == null)
                return NotFound(new { Message = "Notification not found." });

            await _notificationService.DeleteNotification(notification);
            return NoContent();
        }

        /// <summary>
        /// Deletes all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications to delete.</param>
        /// <response code="204">All notifications deleted.</response>
        /// <response code="404">No notifications exist for the specified user.</response>
        [HttpDelete("all/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAllNotifications(int userId)
        {
            var notifications = await _notificationService.GetNotifications(userId);
            if (notifications == null || !notifications.Any())
                return NotFound(new { Message = "No notifications found." });

            await _notificationService.DeleteAllNotifications(userId);
            return NoContent();
        }

        /// <summary>
        /// Updates the message content of an existing notification.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to update.</param>
        /// <param name="updatedNotification">The new notification data (only Message is used).</param>
        /// <response code="204">Notification updated.</response>
        /// <response code="400">Invalid payload.</response>
        /// <response code="404">Notification not found.</response>
        [HttpPut("{notificationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateNotification(int notificationId, [FromBody] NotificationDTO updatedNotification)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = await _notificationService.GetNotification(notificationId);
            if (notification == null)
                return NotFound(new { Message = "Notification not found." });

            notification.Message = updatedNotification.Message;
            await _notificationService.UpdateNotification(notificationId, notification);

            return NoContent();
        }
    }
}
