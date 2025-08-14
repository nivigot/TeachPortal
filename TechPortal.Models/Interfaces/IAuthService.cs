using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechPortal.Models.Models;

namespace TechPortal.Models.Interfaces
{
    public interface IAuthService
    {
        Task<Result<string>> SignupAsync(Teacher teacher);
        Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct);
    }
}
