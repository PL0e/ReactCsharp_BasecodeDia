namespace ASI.Basecode.WebApp.Models.Api
{
    public class StudentSummaryResponse
    {
        public int StudentId { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? YearLevelId { get; set; }
        public string YearLevelName { get; set; }
    }

    public class StudentEnrollmentResponse
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int? Units { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; }
        public string SchoolYear { get; set; }
        public string Status { get; set; }
        public string CurrentGrade { get; set; }
    }

    public class StudentGradeResponse
    {
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; }
        public string SchoolYear { get; set; }
        public string GradeValue { get; set; }
        public string CurrentGrade { get; set; }
        public int? Units { get; set; }
        public int NumberOfTakes { get; set; }
    }

    public class StudentAssignedAdviserResponse
    {
        public int AdviserId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int YearLevelId { get; set; }
        public string YearLevelName { get; set; }
    }

    public class StudentDashboardResponse
    {
        public int StudentId { get; set; }
        public int? YearLevelId { get; set; }
        public string YearLevelName { get; set; }
        public System.Collections.Generic.List<StudentEnrollmentResponse> Enrollments { get; set; } = new();
        public System.Collections.Generic.List<StudentGradeResponse> Grades { get; set; } = new();
        public System.Collections.Generic.List<StudentAssignedAdviserResponse> AssignedAdvisers { get; set; } = new();
    }

    public class StudentAdviserAvailabilityResponse
    {
        public int AdviserId { get; set; }
        public string AdviserName { get; set; }
        public string DayOfWeek { get; set; }
        public System.TimeSpan? StartTime { get; set; }
        public System.TimeSpan? EndTime { get; set; }
        public string Location { get; set; }
        public int SessionMinutes { get; set; }
        public System.Collections.Generic.List<string> Slots { get; set; } = new();
    }
}
