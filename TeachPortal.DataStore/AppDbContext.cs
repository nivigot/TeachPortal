using Microsoft.EntityFrameworkCore;
using TeachPortal.Models.Models;

namespace TeachPortal.DataStore
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Student> Students => Set<Student>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teacher", "dbo");
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.UserName).IsUnique();
                entity.HasIndex(t => t.Email).IsUnique();
                entity.Property(t => t.UserName).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Email).IsRequired().HasMaxLength(256);
                entity.Property(t => t.PasswordHash).IsRequired();
                entity.Property(t => t.FirstName).HasMaxLength(50);
                entity.Property(t => t.LastName).HasMaxLength(50);

                entity.HasMany(t => t.Students)
                      .WithOne(s => s.Teacher)
                      .HasForeignKey(s => s.TeacherId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Student", "dbo");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(s => s.LastName).IsRequired().HasMaxLength(50);
                entity.Property(s => s.Email).IsRequired().HasMaxLength(256);
            });
        }
    }
}
