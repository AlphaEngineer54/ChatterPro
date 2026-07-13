using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Models.DTO.User;
using UserService.Services;

namespace UserService.Controllers
{
    /// <summary>
    /// Manages non-sensitive user information (retrieve, update, delete).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AccountService _userService;

        public UserController(AccountService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>User information DTO.</returns>
        /// <response code="200">User found and returned.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserInfo(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            return Ok(user);
        }

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>User information DTO.</returns>
        /// <response code="200">User found and returned.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("by-username/{username}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUserName(username);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            return Ok(user);
        }

        /// <summary>
        /// Updates an existing user’s information.
        /// </summary>
        /// <param name="user">DTO containing updated user fields.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">User updated successfully.</response>
        /// <response code="400">Invalid payload.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _userService.UpdateUser(user);
            if (updated)
                return NoContent();

            return NotFound(new { Message = "User not found." });
        }

        /// <summary>
        /// Deletes a user by their unique identifier.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">User deleted successfully.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Failed to delete user.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserInfo(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            var deleted = await _userService.DeleteUser(user);
            if (deleted)
                return NoContent();

            return StatusCode(500, new { Message = "User could not be deleted." });
        }
    }
}
