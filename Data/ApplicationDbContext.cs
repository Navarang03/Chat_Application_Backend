using Microsoft.EntityFrameworkCore;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<AcademicQualification> AcademicQualifications { get; set; }
        public DbSet<Languages> Languages { get; set; }
        public DbSet<Signup> Signups { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Qualifications)
                .WithOne()
                .HasForeignKey(q => q.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade); // ?? Cascade

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.LanguagesKnown)
                .WithOne()
                .HasForeignKey<Languages>(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade); // ?? Cascade

            base.OnModelCreating(modelBuilder);
        }

    }
}
