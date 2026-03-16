using TeachPortal.Models.Models;

namespace TeachPortal.Models.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherOverview>> GetTeachersAsync(CancellationToken ct = default);
    }
}
