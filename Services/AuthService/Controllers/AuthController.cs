using AuthService.Events;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

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
                var user = new User()
                {
                    Email = loginDTO.Email,
                    Password = loginDTO.Password,
                };

                var isUserAuthentified = await this._userService.AuthenficateUser(user);

                this._logger.LogInformation(isUserAuthentified ? $"New user authentificated successfully {user}" :
                                                                 $"Failed to authentificate {user}");
                return isUserAuthentified ? Ok(new {  JWTToken = this._jwtService.GenerateJWTToken(user) }) :
                                            Unauthorized(new { Message = $"Invalid credential for user {user.Email}" });
            }

            return BadRequest(ModelState);
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> CreateUser([FromBody] SignInDTO signInDTO)
        {
            if (ModelState.IsValid)
            {
                var user = new User()
                {
                    Email = signInDTO.Email,
                    Password = signInDTO.Password,
                };

                var isUserCreated = await this._userService.CreateUser(user);

                if (isUserCreated)
                {
                    var userCreatedEvent = CreateUserEvent(user, signInDTO.UserName ?? "unknown-user");
                    this._producerService.PublishEvent(userCreatedEvent, "user-created");
                    this._logger.LogInformation($"New user created successfully! {user}");

                    return CreatedAtAction(nameof(CreateUser), new
                    {
                        JWTToken = _jwtService.GenerateJWTToken(user)
                    });
                }
            }

            return BadRequest(new { Message = $"Failed to create a new user: {signInDTO.Email}", ModelError = ModelState});
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

