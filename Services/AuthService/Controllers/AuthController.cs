using AuthService.Events;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace AuthService.Controllers
{
    /// <summary>
    /// Handles user authentication and account creation.
    /// </summary>
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

        /// <summary>
        /// Authenticates a user and returns a JWT token upon success.
        /// </summary>
        /// <param name="loginDTO">User login credentials.</param>
        /// <returns>Returns the user ID, email, and a signed JWT token.</returns>
        /// <response code="200">Authentication successful. Returns user info and JWT.</response>
        /// <response code="400">Request is invalid or missing required fields.</response>
        /// <response code="401">Authentication failed due to invalid credentials.</response>
        /// <response code="500">Unexpected error during authentication process.</response>
        [HttpPost("login")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

                    this._logger.LogInformation($"New user authenticated successfully {user}");

                    return Ok(new
                    {
                        User = new { loginUser.Id, loginUser.Email },
                        JWTToken = this._jwtService.GenerateJWTToken(loginUser)
                    });
                }
                catch (InvalidCredentialException ex)
                {
                    this._logger.LogWarning($"Authentication failed: {ex.Message}");
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

        /// <summary>
        /// Creates a new user account and returns a JWT token.
        /// </summary>
        /// <param name="signInDTO">User registration information.</param>
        /// <returns>Returns the created user info and a JWT token.</returns>
        /// <remarks>
        /// If the account is successfully created, an event is published to the "user-created" topic.
        /// </remarks>
        /// <response code="201">User successfully created. JWT token included.</response>
        /// <response code="400">Request is invalid or contains invalid fields.</response>
        /// <response code="409">Email is already in use by another account.</response>
        /// <response code="500">Unexpected error during account creation.</response>
        [HttpPost("signUp")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                            JWTToken = _jwtService.GenerateJWTToken(createdUser)
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

        /// <summary>
        /// Creates an internal user creation event to be published.
        /// </summary>
        /// <param name="user">User entity object.</param>
        /// <param name="userName">Username provided by the user.</param>
        /// <returns>The constructed user-created event.</returns>
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


