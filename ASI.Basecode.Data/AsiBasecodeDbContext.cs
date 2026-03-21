using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Adviser> Advisers { get; set; }
        public virtual DbSet<YearLevel> YearLevels { get; set; }
        public virtual DbSet<AdviserAssignment> AdviserAssignments { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Semester> Semesters { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<AppointmentNote> AppointmentNotes { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
        public virtual DbSet<AdviserAssignmentHistory> AdviserAssignmentHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(255);
                entity.Property(e => e.Role).HasMaxLength(50);
            });

            modelBuilder.Entity<Adviser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
            });

            modelBuilder.Entity<YearLevel>(entity =>
            {
                entity.Property(e => e.YearName).HasMaxLength(50);
            });

            modelBuilder.Entity<AdviserAssignment>(entity =>
            {
                entity.HasIndex(e => new { e.AdviserId, e.YearLevelId }).IsUnique();
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(e => e.CourseCode).HasMaxLength(20);
                entity.Property(e => e.CourseName).HasMaxLength(150);
            });

            modelBuilder.Entity<Semester>(entity =>
            {
                entity.Property(e => e.SemesterName).HasMaxLength(50);
                entity.Property(e => e.SchoolYear).HasMaxLength(20);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.Property(e => e.AppointmentType).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("UPCOMING");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<AppointmentNote>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Grade>(entity =>
            {
                entity.Property(e => e.GradeValue).HasMaxLength(10);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<AdviserAssignmentHistory>(entity =>
            {
                entity.Property(e => e.Action).HasMaxLength(20);
                entity.Property(e => e.ActionDate).HasDefaultValueSql("GETDATE()");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
