namespace TeachPortal.Models.Models
{
    public class TeacherOverview
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public int StudentCount { get; set; }
    }
}
