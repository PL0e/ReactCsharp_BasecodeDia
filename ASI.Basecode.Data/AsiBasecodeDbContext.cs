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
        public virtual DbSet<Enrollment> Enrollments { get; set; }
        public virtual DbSet<AdviserAvailability> AdviserAvailabilities { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<AppointmentNote> AppointmentNotes { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("userID");
                entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100);
                entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(255);
                entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50);
                entity.Property(e => e.FirstName).HasColumnName("firstName").HasMaxLength(100);
                entity.Property(e => e.LastName).HasColumnName("lastName").HasMaxLength(100);
                entity.Property(e => e.IsFirstLogin).HasColumnName("isFirstLogin").HasDefaultValue(true);
                entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<YearLevel>(entity =>
            {
                entity.ToTable("YearLevels");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("yearLevelID");
                entity.Property(e => e.YearName).HasColumnName("yearName").HasMaxLength(50);
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("studentID");
                entity.Property(e => e.UserId).HasColumnName("userID");
                entity.Property(e => e.YearLevelId).HasColumnName("yearLevelID");
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            modelBuilder.Entity<Adviser>(entity =>
            {
                entity.ToTable("Advisers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("adviserID");
                entity.Property(e => e.UserId).HasColumnName("userID");
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            modelBuilder.Entity<Semester>(entity =>
            {
                entity.ToTable("Semester");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("semesterID");
                entity.Property(e => e.SemesterName).HasColumnName("semesterName").HasMaxLength(50);
                entity.Property(e => e.SchoolYear).HasColumnName("schoolYear").HasMaxLength(20);
                entity.Property(e => e.IsCurrent).HasColumnName("isCurrent").HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(false);
                entity.Property(e => e.StartDate).HasColumnName("startDate").HasMaxLength(50);
                entity.Property(e => e.EndDate).HasColumnName("endDate").HasMaxLength(50);
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("courseID");
                entity.Property(e => e.CourseCode).HasColumnName("courseCode").HasMaxLength(20);
                entity.Property(e => e.CourseName).HasColumnName("courseName").HasMaxLength(150);
                entity.Property(e => e.Units).HasColumnName("units");
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("Enrollments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("enrollmentID");
                entity.Property(e => e.StudentId).HasColumnName("studentID");
                entity.Property(e => e.CourseId).HasColumnName("courseID");
                entity.Property(e => e.SemesterId).HasColumnName("semesterID");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grades");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("gradeID");
                entity.Property(e => e.StudentId).HasColumnName("studentID");
                entity.Property(e => e.CourseId).HasColumnName("courseID");
                entity.Property(e => e.SemesterId).HasColumnName("semesterID");
                entity.Property(e => e.GradeValue).HasColumnName("gradeValue").HasColumnType("decimal(3, 2)");
                entity.Property(e => e.Units).HasColumnName("units");
                entity.Property(e => e.NumberOfTakes).HasColumnName("numberOfTakes").HasDefaultValue(1);
                entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            modelBuilder.Entity<AdviserAssignment>(entity =>
            {
                entity.ToTable("AdviserAssignments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("assignmentID");
                entity.Property(e => e.AdviserId).HasColumnName("adviserID");
                entity.Property(e => e.YearLevelId).HasColumnName("yearLevelID");
                entity.Property(e => e.AssignedAt).HasColumnName("assignedAt").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
                entity.HasIndex(e => new { e.AdviserId, e.YearLevelId }).IsUnique();
            });

            modelBuilder.Entity<AdviserAvailability>(entity =>
            {
                entity.ToTable("AdviserAvailability");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("availabilityID");
                entity.Property(e => e.AdviserId).HasColumnName("adviserID");
                entity.Property(e => e.DayOfWeek).HasColumnName("dayOfWeek").HasMaxLength(20);
                entity.Property(e => e.StartTime).HasColumnName("startTime");
                entity.Property(e => e.EndTime).HasColumnName("endTime");
                entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(100);
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("appointmentID");
                entity.Property(e => e.StudentId).HasColumnName("studentID");
                entity.Property(e => e.AdviserId).HasColumnName("adviserID");
                entity.Property(e => e.SemesterId).HasColumnName("semesterID");
                entity.Property(e => e.AppointmentType).HasColumnName("appointmentType").HasMaxLength(50);
                entity.Property(e => e.AppointmentDate).HasColumnName("appointmentDate");
                entity.Property(e => e.AppointmentTime).HasColumnName("appointmentTime");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("UPCOMING");
                entity.Property(e => e.CancellationReason).HasColumnName("cancellationReason");
                entity.Property(e => e.CancellationDate).HasColumnName("cancellationDate");
                entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            modelBuilder.Entity<AppointmentNote>(entity =>
            {
                entity.ToTable("AppointmentNotes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("noteID");
                entity.Property(e => e.AppointmentId).HasColumnName("appointmentID");
                entity.Property(e => e.AdviserId).HasColumnName("adviserID");
                entity.Property(e => e.AdviserNotes).HasColumnName("adviserNotes");
                entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted").HasDefaultValue(false);
                entity.Property(e => e.DeleteDate).HasColumnName("deleteDate");
                entity.Property(e => e.DeleteName).HasColumnName("deleteName").HasMaxLength(100);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
