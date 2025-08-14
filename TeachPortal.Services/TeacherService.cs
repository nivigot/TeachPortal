using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeachPortal.DataStore;
using TechPortal.Models.Interfaces;
using TechPortal.Models.Models;

namespace TeachPortal.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TeacherService> _logger;
        public TeacherService(AppDbContext dbContext, ILogger<TeacherService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger;
        }
        public async Task<IEnumerable<TeacherOverview>> GetTeachersAsync()
        {
            return await _dbContext.Teachers.Select(t => new TeacherOverview {
                Id = t.Id,
                Name = $"{t.FirstName} {t.LastName}",
                StudentCount = t.Students.Count
            }).ToListAsync();
        }
    }
}
