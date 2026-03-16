using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeachPortal.DataStore;
using TeachPortal.Models.Interfaces;
using TeachPortal.Models.Models;

namespace TeachPortal.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(AppDbContext dbContext, ILogger<TeacherService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<TeacherOverview>> GetTeachersAsync(CancellationToken ct = default)
        {
            return await _dbContext.Teachers
                .AsNoTracking()
                .Select(t => new TeacherOverview
                {
                    Id = t.Id,
                    Name = (t.FirstName + " " + t.LastName).Trim(),
                    Email = t.Email,
                    UserName = t.UserName,
                    StudentCount = t.Students != null ? t.Students.Count : 0
                })
                .ToListAsync(ct);
        }
    }
}
