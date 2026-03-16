using TeachPortal.Models.Models;

namespace TeachPortal.Models.Interfaces
{
    public interface IAuthService
    {
        Task<Result<string>> SignupAsync(Teacher teacher);
        Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}
