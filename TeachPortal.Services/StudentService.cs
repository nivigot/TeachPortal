using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeachPortal.DataStore;
using TeachPortal.Models.Interfaces;
using TeachPortal.Models.Models;

namespace TeachPortal.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<StudentService> _logger;

        public StudentService(AppDbContext dbContext, ILogger<StudentService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Student>> CreateStudentAsync(Student student, int teacherId)
        {
            try
            {
                if (student is null)
                {
                    _logger.LogWarning("Attempted to create a student with null data.");
                    return new Result<Student>(false, "Invalid student data.", statusCode: 400);
                }

                var teacher = await _dbContext.Teachers.FindAsync(teacherId);
                if (teacher is null)
                {
                    _logger.LogWarning("Teacher not found: {TeacherId}", teacherId);
                    return new Result<Student>(false, "Teacher not found.", statusCode: 404);
                }

                student.Teacher = teacher;
                await _dbContext.Students.AddAsync(student);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Student created successfully: {StudentId} for teacher: {TeacherId}", student.Id, teacherId);
                return new Result<Student>(true, "Student created successfully.", student, statusCode: 201);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating student for teacher: {TeacherId}", teacherId);
                return new Result<Student>(false, "A database error occurred while creating the student.", statusCode: 500);
            }
        }

        public async Task<IEnumerable<Student>> GetStudentsByTeacherAsync(int teacherId, CancellationToken ct = default)
        {
            return await _dbContext.Students
                .AsNoTracking()
                .Where(s => s.TeacherId == teacherId)
                .ToListAsync(ct);
        }
    }
}
