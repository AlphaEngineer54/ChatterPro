using Microsoft.AspNetCore.Mvc;
using UserService.Services;
using UserService.Models.DTO.User;
using Microsoft.AspNetCore.Authorization;
using UserService.Interfaces;

namespace UserService.Controllers
{
    /// <summary>
    ///  This REST API manage the non-sensible information about users
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

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserInfo(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET : api/user{username}
        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUserName(username);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateUser(user);

            if (result)
            {
                return NoContent();
            }

            return NotFound($"User not found.");
        }

        // DELETE: api/User/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserInfo(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userService.DeleteUser(user);

            if (result)
            {
                return NoContent();
            }

            return StatusCode(500, "User could not be deleted.");
        }
    }
}
