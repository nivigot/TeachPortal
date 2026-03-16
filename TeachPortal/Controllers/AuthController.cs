using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachPortal.Models.Interfaces;
using TeachPortal.Models.Models;

namespace TeachPortal.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("signup")]
        [SwaggerOperation(Summary = "Register a new teacher", Description = "Creates a teacher account with a hashed password.")]
        [SwaggerResponse(201, "Teacher registered successfully.")]
        [SwaggerResponse(400, "Validation error in the request payload.")]
        [SwaggerResponse(409, "Email or username is already registered.")]
        [SwaggerResponse(500, "Unexpected server error.")]
        public async Task<IActionResult> SignupAsync([FromBody] Teacher teacher, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _authService.SignupAsync(teacher);

            if (!result.Success)
            {
                _logger.LogWarning("Signup unsuccessful: {Message}", result.Message);
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return StatusCode(201, new { message = result.Message });
        }

        [HttpPost("login")]
        [SwaggerOperation(Summary = "Log in as a teacher", Description = "Validates credentials and returns a signed JWT.")]
        [SwaggerResponse(200, "Login successful. Returns a JWT token.")]
        [SwaggerResponse(400, "Missing or malformed request payload.")]
        [SwaggerResponse(401, "Invalid username or password.")]
        [SwaggerResponse(500, "Unexpected server error.")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _authService.LoginAsync(request, ct);

            if (!result.Success)
            {
                _logger.LogWarning("Login unsuccessful for '{UserName}': {Message}", request.Username, result.Message);
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(result.Data);
        }
    }
}
