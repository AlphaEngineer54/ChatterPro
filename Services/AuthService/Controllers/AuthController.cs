using AuthService.Events;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace AuthService.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(ILogger<AuthController> logger,
                          UserService userService,
                          JWTService jwtService,
                          ProducerService producer) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly UserService _userService = userService;
        private readonly JWTService _jwtService = jwtService;
        private readonly ProducerService _producerService = producer;

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateUser([FromBody] LoginDTO loginDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new User()
                    {
                        Email = loginDTO.Email,
                        Password = loginDTO.Password,
                    };

                    var loginUser = await this._userService.AuthenticateUser(user);

                    this._logger.LogInformation($"New user authentificated successfully {user}");

                    return Ok(new
                    {
                        User = new { loginUser.Id, loginUser.Email },
                        JWTToken = this._jwtService.GenerateJWTToken(user)
                    });
                }
                catch(InvalidCredentialException ex)
                {
                    this._logger.LogWarning($"Authenfication failed: {ex.Message}");
                    return Unauthorized(new { ex.Message });
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"An error occurred while authenticating the user: {ex.Message}");
                    return StatusCode(500, new { Message = "An error occurred while authenticating the user." });
                }

            }

            return BadRequest(ModelState);
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> CreateUser([FromBody] SignInDTO signInDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new User()
                    {
                        Email = signInDTO.Email,
                        Password = signInDTO.Password,
                    };

                    var createdUser = await this._userService.CreateUser(user);

                    if (createdUser != null)
                    {
                        var userCreatedEvent = CreateUserEvent(user, signInDTO.UserName ?? "unknown-user");
                        this._producerService.PublishEvent(userCreatedEvent, "user-created");
                        this._logger.LogInformation($"New user created successfully! {user}");

                        return CreatedAtAction(nameof(CreateUser), new
                        {
                            User = new { createdUser.Id, createdUser.Email },
                            JWTToken = _jwtService.GenerateJWTToken(user)
                        });
                    }
                }
                catch (InvalidCredentialException ex)
                {
                    this._logger.LogWarning($"Conflict email detected: {ex.Message}");
                    return Conflict(new { ex.Message });
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"An error occurred while creating the user: {ex.Message}");
                    return StatusCode(500, new { Message = "An error occurred while creating the user." });
                }
            }

            return BadRequest(ModelState);
        }

        private UserCreatedEvent CreateUserEvent(User user, string userName)
        {
            return new UserCreatedEvent()
            {
                Id = user.Id,
                UserName = userName,
            };
        }
    }
}

