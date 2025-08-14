using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechPortal.Models.Interfaces;
using TechPortal.Models.Models;

namespace TeachPortal.Controllers
{


    [Route("api/teacher")]
    [ApiController]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ILogger<TeacherController> _logger;
        private readonly IStudentService _studentService;

        public TeacherController(ITeacherService teacherService, IStudentService studentService,ILogger<TeacherController> logger)
        {
            _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
            _studentService = studentService ?? throw new Exception(nameof(studentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherOverview>>> GetTeachersAsync()
        {
            try
            {
                var teachers = await _teacherService.GetTeachersAsync();
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving teachers");
                return StatusCode(403, "An error occurred while retrieving teachers");
            }
        }

        [HttpGet("{teacherId:int}/students")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudentsForTeacher(int teacherId)
        {
            // Optional: only allow self or admins
            var currentIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!int.TryParse(currentIdStr, out var currentId))
                return Unauthorized();

            var isSelf = currentId == teacherId;
            var isAdmin = User.IsInRole("Admin"); // depends on your auth setup
            if (!isSelf && !isAdmin) return Forbid();

            var students = await _studentService.GetStudentsByTeacherAsync(teacherId);
            return Ok(students);
        }
    }

}