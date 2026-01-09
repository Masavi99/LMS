using LMS.Models.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Course> Course { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<QuizQuestion> QuizQuestion { get; set; }
        public DbSet<QuizResult> QuizResult { get; set; }
        public DbSet<ProgressTracking> ProgressTracking { get; set; }
        public DbSet<FinalExam> FinalExam { get; set; }
        public DbSet<FinalExamQuestion> FinalExamQuestion { get; set; }
        public DbSet<FinalExamSubmission> FinalExamSubmission { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<StudentQuizAnswer> StudentQuizAnswer { get; set; }
        public DbSet<StudentFinalExamAnswer> StudentFinalExamAnswer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            // ProgressTracking config
            modelBuilder.Entity<ProgressTracking>()
                .Property(p => p.CompletionDate)
                .HasColumnType("datetime2"); // safer range/precision

            modelBuilder.Entity<ProgressTracking>()
                .HasIndex(p => new { p.StudentId, p.ModuleId })
                .IsUnique();
        }
    }
}
