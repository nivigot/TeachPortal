using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachPortal.Models.Interfaces;
using TeachPortal.Models.Models;

namespace TeachPortal.Controllers
{
    [Route("api/students")]
    [ApiController]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Add a student", Description = "Creates a new student and assigns them to the authenticated teacher.")]
        [SwaggerResponse(201, "Student created successfully.")]
        [SwaggerResponse(400, "Validation error in the request payload.")]
        [SwaggerResponse(401, "Missing or invalid teacher claim in token.")]
        [SwaggerResponse(500, "Unexpected server error.")]
        public async Task<ActionResult> CreateStudentAsync([FromBody] Student student, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var teacherId))
                return Unauthorized(new { message = "Missing or invalid teacher id claim." });

            var result = await _studentService.CreateStudentAsync(student, teacherId);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return StatusCode(201, result.Data);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get my students", Description = "Returns all students assigned to the authenticated teacher.")]
        [SwaggerResponse(200, "Students retrieved successfully.")]
        [SwaggerResponse(401, "Missing or invalid teacher claim in token.")]
        [SwaggerResponse(500, "Unexpected server error.")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudentsAsync(CancellationToken ct)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var teacherId))
                return Unauthorized(new { message = "Missing or invalid teacher id claim." });

            try
            {
                var students = await _studentService.GetStudentsByTeacherAsync(teacherId, ct);
                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students for teacher {TeacherId}.", teacherId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving students." });
            }
        }
    }
}
