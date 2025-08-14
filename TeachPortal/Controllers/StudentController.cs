using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TechPortal.Models.Interfaces;
using TechPortal.Models.Models;

namespace TeachPortal.Controllers
{
    [Route("api/students")]
    [ApiController]
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new student for the authenticated teacher.
        /// </summary>
        /// <param name="student">The student to create.</param>
        /// <returns>A response indicating the result of the student creation.</returns>
        /// <response code="200">Student created successfully.</response>
        /// <response code="400">Invalid student data.</response>
        /// <response code="500">Error occurred while creating the student.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new student", Description = "Creates a new student for the authenticated teacher.")]
        [SwaggerResponse(200, "Student created successfully.")]
        [SwaggerResponse(400, "Invalid student data.")]
        [SwaggerResponse(500, "Error occurred while creating the student.")]
        public async Task<ActionResult> CreateStudentAsync([FromBody] Student student)
        {
            try
            {
                if (!ModelState.IsValid) return ValidationProblem(ModelState);

                var idClaim =
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var teacherId))
                    return Unauthorized("Missing or invalid teacher id claim.");

                var result = await _studentService.CreateStudentAsync(student, teacherId);
                if (result == null) return StatusCode(500, "Unknown error");
                if (!result.Success) return StatusCode(500, result.Message);

                return Ok(result.Student);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid student data.");
                return BadRequest("Invalid student data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the student.");
                return StatusCode(500, "An error occurred while creating the student.");
            }
        }

        /// <summary>
        /// Retrieves the list of students for the authenticated teacher.
        /// </summary>
        /// <returns>A list of students.</returns>
        /// <response code="200">List of students retrieved successfully.</response>
        /// <response code="500">Error occurred while retrieving the students.</response>
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieves the list of students", Description = "Retrieves the list of students for the authenticated teacher.")]
        [SwaggerResponse(200, "List of students retrieved successfully.")]
        [SwaggerResponse(500, "Error occurred while retrieving the students.")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            try
            {
                var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var students = await _studentService.GetStudentsByTeacherAsync(teacherId);
                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the students.");
                return StatusCode(500, "An error occurred while retrieving the students.");
            }
        }
    }
}
