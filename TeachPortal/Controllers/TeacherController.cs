using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachPortal.Models.Interfaces;
using TeachPortal.Models.Models;

namespace TeachPortal.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly IStudentService _studentService;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            ITeacherService teacherService,
            IStudentService studentService,
            ILogger<TeacherController> logger)
        {
            _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all teachers", Description = "Returns a summary of all teachers with their student counts.")]
        [SwaggerResponse(200, "Teachers retrieved successfully.")]
        [SwaggerResponse(500, "Unexpected server error.")]
        public async Task<ActionResult<IEnumerable<TeacherOverview>>> GetTeachersAsync(CancellationToken ct)
        {
            try
            {
                var teachers = await _teacherService.GetTeachersAsync(ct);
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving teachers.");
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving teachers." });
            }
        }

        [HttpGet("{teacherId:int}/students")]
        [SwaggerOperation(Summary = "Get students for a teacher", Description = "Returns the student list for the given teacher. A teacher may only view their own students unless they have the Admin role.")]
        [SwaggerResponse(200, "Students retrieved successfully.")]
        [SwaggerResponse(401, "Missing or invalid token.")]
        [SwaggerResponse(403, "Access denied. Teachers may only view their own students.")]
        [SwaggerResponse(500, "Unexpected server error.")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudentsForTeacherAsync(int teacherId, CancellationToken ct)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(idClaim, out var currentId))
                return Unauthorized(new { message = "Missing or invalid teacher id claim." });

            if (currentId != teacherId && !User.IsInRole("Admin"))
                return Forbid();

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
