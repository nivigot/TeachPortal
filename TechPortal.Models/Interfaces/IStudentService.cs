using TeachPortal.Models.Models;

namespace TeachPortal.Models.Interfaces
{
    public interface IStudentService
    {
        Task<Result<Student>> CreateStudentAsync(Student student, int teacherId);
        Task<IEnumerable<Student>> GetStudentsByTeacherAsync(int teacherId, CancellationToken ct = default);
    }
}
