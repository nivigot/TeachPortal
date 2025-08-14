using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechPortal.Models.Models
{
    public class TeacherOverview
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int StudentCount { get; set; }
    }
}
