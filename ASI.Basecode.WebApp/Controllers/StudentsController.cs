using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Api;
using ASI.Basecode.WebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StudentsController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;
        private readonly NotificationStreamManager _notificationStreamManager;
        private static readonly HashSet<string> AllowedAvailabilityDays = new(StringComparer.OrdinalIgnoreCase)
        {
            "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY"
        };
        private const int AppointmentSessionMinutes = 30;

        public StudentsController(AsiBasecodeDBContext context, NotificationStreamManager notificationStreamManager)
        {
            _context = context;
            _notificationStreamManager = notificationStreamManager;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StudentSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StudentSummaryResponse>>> GetAll([FromQuery] int? yearLevelId = null)
        {
            var currentRole = User?.FindFirst(ClaimTypes.Role)?.Value?.ToUpperInvariant();
            var isAdviserScoped = currentRole == "ADVISER";

            var scopedYearLevelIds = new List<int>();
            if (isAdviserScoped)
            {
                scopedYearLevelIds = await GetAssignedYearLevelIdsForCurrentAdviserAsync();
                if (scopedYearLevelIds.Count == 0)
                {
                    return Ok(new List<StudentSummaryResponse>());
                }
            }

            var students = await (
                from s in _context.Students.AsNoTracking()
                where !s.IsDeleted
                    && (!isAdviserScoped || (s.YearLevelId.HasValue && scopedYearLevelIds.Contains(s.YearLevelId.Value)))
                    && (!yearLevelId.HasValue || s.YearLevelId == yearLevelId.Value)
                join y in _context.YearLevels.AsNoTracking() on s.YearLevelId equals (int?)y.Id into yearLevels
                from y in yearLevels.DefaultIfEmpty()
                join u in _context.Users.AsNoTracking().Where(x => x.IsActive && x.Role == "STUDENT") on s.UserId equals (int?)u.Id into users
                from u in users.DefaultIfEmpty()
                select new StudentSummaryResponse
                {
                    StudentId = s.Id,
                    UserId = s.UserId,
                    Username = u != null ? u.Username : null,
                    FirstName = u != null ? u.FirstName : null,
                    LastName = u != null ? u.LastName : null,
                    Email = u != null ? u.Email : null,
                    YearLevelId = s.YearLevelId,
                    YearLevelName = y != null ? y.YearName : null
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("{studentId:int}")]
        [ProducesResponseType(typeof(StudentSummaryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentSummaryResponse>> GetById(int studentId)
        {
            var currentRole = User?.FindFirst(ClaimTypes.Role)?.Value?.ToUpperInvariant();
            var isAdviserScoped = currentRole == "ADVISER";

            var scopedYearLevelIds = new List<int>();
            if (isAdviserScoped)
            {
                scopedYearLevelIds = await GetAssignedYearLevelIdsForCurrentAdviserAsync();
                if (scopedYearLevelIds.Count == 0)
                {
                    return NotFound();
                }
            }

            var student = await (
                from s in _context.Students.AsNoTracking()
                where s.Id == studentId && !s.IsDeleted
                    && (!isAdviserScoped || (s.YearLevelId.HasValue && scopedYearLevelIds.Contains(s.YearLevelId.Value)))
                join y in _context.YearLevels.AsNoTracking() on s.YearLevelId equals (int?)y.Id into yearLevels
                from y in yearLevels.DefaultIfEmpty()
                join u in _context.Users.AsNoTracking().Where(x => x.IsActive && x.Role == "STUDENT") on s.UserId equals (int?)u.Id into users
                from u in users.DefaultIfEmpty()
                select new StudentSummaryResponse
                {
                    StudentId = s.Id,
                    UserId = s.UserId,
                    Username = u != null ? u.Username : null,
                    FirstName = u != null ? u.FirstName : null,
                    LastName = u != null ? u.LastName : null,
                    Email = u != null ? u.Email : null,
                    YearLevelId = s.YearLevelId,
                    YearLevelName = y != null ? y.YearName : null
                })
                .FirstOrDefaultAsync();

            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpGet("me/assigned-advisers")]
        [ProducesResponseType(typeof(IEnumerable<StudentAssignedAdviserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StudentAssignedAdviserResponse>>> GetMyAssignedAdvisers()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null || !student.YearLevelId.HasValue)
            {
                return Ok(new List<StudentAssignedAdviserResponse>());
            }

            var advisers = await GetAssignedAdvisersByYearLevelAsync(student.YearLevelId.Value);

            return Ok(advisers);
        }

        [HttpGet("me/dashboard")]
        [ProducesResponseType(typeof(StudentDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentDashboardResponse>> GetMyDashboard()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                return NotFound();
            }

            var yearLevelName = student.YearLevelId.HasValue
                ? await _context.YearLevels.AsNoTracking()
                    .Where(y => y.Id == student.YearLevelId.Value && !y.IsDeleted)
                    .Select(y => y.YearName)
                    .FirstOrDefaultAsync()
                : null;

            var enrollments = await (from enrollment in _context.Enrollments.AsNoTracking()
                                     join course in _context.Courses.AsNoTracking() on enrollment.CourseId equals course.Id into courseJoin
                                     from course in courseJoin.DefaultIfEmpty()
                                     join semester in _context.Semesters.AsNoTracking() on enrollment.SemesterId equals semester.Id into semesterJoin
                                     from semester in semesterJoin.DefaultIfEmpty()
                                     where enrollment.StudentId == student.Id && !enrollment.IsDeleted
                                     select new StudentEnrollmentResponse
                                     {
                                         EnrollmentId = enrollment.Id,
                                         StudentId = enrollment.StudentId,
                                         CourseId = enrollment.CourseId,
                                         CourseCode = course == null ? null : course.CourseCode,
                                         CourseName = course == null ? null : course.CourseName,
                                         Units = course == null ? null : (int?)course.Units,
                                         SemesterId = enrollment.SemesterId,
                                         SemesterName = semester == null ? null : semester.SemesterName,
                                         SchoolYear = semester == null ? null : semester.SchoolYear,
                                         Status = enrollment.Status,
                                         CurrentGrade = _context.Grades.AsNoTracking()
                                             .Where(g => g.StudentId == enrollment.StudentId
                                                         && g.CourseId == enrollment.CourseId
                                                         && g.SemesterId == enrollment.SemesterId
                                                         && !g.IsDeleted)
                                             .OrderByDescending(g => g.CreatedAt)
                                             .Select(g => g.GradeValue)
                                             .FirstOrDefault()
                                     })
                .ToListAsync();

            var grades = await (from grade in _context.Grades.AsNoTracking()
                                join course in _context.Courses.AsNoTracking() on grade.CourseId equals course.Id into courseJoin
                                from course in courseJoin.DefaultIfEmpty()
                                join semester in _context.Semesters.AsNoTracking() on grade.SemesterId equals semester.Id into semesterJoin
                                from semester in semesterJoin.DefaultIfEmpty()
                                where grade.StudentId == student.Id && !grade.IsDeleted
                                select new StudentGradeResponse
                                {
                                    GradeId = grade.Id,
                                    StudentId = grade.StudentId,
                                    CourseId = grade.CourseId,
                                    CourseCode = course == null ? null : course.CourseCode,
                                    CourseName = course == null ? null : course.CourseName,
                                    SemesterId = grade.SemesterId,
                                    SemesterName = semester == null ? null : semester.SemesterName,
                                    SchoolYear = semester == null ? null : semester.SchoolYear,
                                    GradeValue = grade.GradeValue,
                                    CurrentGrade = grade.GradeValue,
                                    Units = grade.Units,
                                    NumberOfTakes = grade.NumberOfTakes
                                })
                .ToListAsync();

            var assignedAdvisers = student.YearLevelId.HasValue
                ? await GetAssignedAdvisersByYearLevelAsync(student.YearLevelId.Value)
                : new List<StudentAssignedAdviserResponse>();

            return Ok(new StudentDashboardResponse
            {
                StudentId = student.Id,
                YearLevelId = student.YearLevelId,
                YearLevelName = yearLevelName,
                Enrollments = enrollments,
                Grades = grades,
                AssignedAdvisers = assignedAdvisers.ToList()
            });
        }

        [HttpGet("me/adviser-availabilities")]
        [ProducesResponseType(typeof(IEnumerable<StudentAdviserAvailabilityResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StudentAdviserAvailabilityResponse>>> GetMyAdviserAvailabilities()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null || !student.YearLevelId.HasValue)
            {
                return Ok(new List<StudentAdviserAvailabilityResponse>());
            }

            var adviserIds = await _context.AdviserAssignments.AsNoTracking()
                .Where(x => !x.IsDeleted && x.YearLevelId == student.YearLevelId.Value)
                .Select(x => x.AdviserId)
                .Distinct()
                .ToListAsync();

            if (adviserIds.Count == 0)
            {
                return Ok(new List<StudentAdviserAvailabilityResponse>());
            }

            var rawAvailabilities = await (
                from availability in _context.AdviserAvailabilities.AsNoTracking()
                join adviser in _context.Advisers.AsNoTracking() on availability.AdviserId equals adviser.Id
                join user in _context.Users.AsNoTracking() on adviser.UserId equals user.Id
                where !availability.IsDeleted
                    && !adviser.IsDeleted
                    && user.IsActive
                    && adviserIds.Contains(availability.AdviserId)
                select new
                {
                    availability.AdviserId,
                    AdviserName = string.IsNullOrWhiteSpace((user.FirstName + " " + user.LastName).Trim())
                        ? user.Username
                        : (user.FirstName + " " + user.LastName).Trim(),
                    availability.DayOfWeek,
                    availability.StartTime,
                    availability.EndTime,
                    availability.Location
                })
                .ToListAsync();

            var items = rawAvailabilities
                .Where(x => IsAllowedAvailabilityDay(x.DayOfWeek))
                .Select(x => new StudentAdviserAvailabilityResponse
                {
                    AdviserId = x.AdviserId,
                    AdviserName = x.AdviserName,
                    DayOfWeek = NormalizeDayOfWeek(x.DayOfWeek),
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Location = x.Location,
                    SessionMinutes = AppointmentSessionMinutes,
                    Slots = BuildThirtyMinuteSlots(x.StartTime, x.EndTime)
                })
                .OrderBy(x => x.AdviserName)
                .ThenBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .ToList();

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<Student>> Create([FromBody] Student student)
        {
            student.IsDeleted = false;
            student.DeleteDate = null;
            student.DeleteName = null;
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { studentId = student.Id }, student);
        }

        [HttpPut("{studentId:int}")]
        public async Task<IActionResult> Update(int studentId, [FromBody] Student student)
        {
            if (studentId != student.Id) return BadRequest();
            var existing = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.UserId = student.UserId;
            existing.YearLevelId = student.YearLevelId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{studentId:int}")]
        public async Task<IActionResult> Delete(int studentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);
            if (student == null) return NotFound();

            student.IsDeleted = true;
            student.DeleteDate = DateTime.Now;
            student.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{studentId:int}/enrollments")]
        [ProducesResponseType(typeof(IEnumerable<StudentEnrollmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<StudentEnrollmentResponse>>> GetStudentEnrollments(int studentId)
        {
            var studentExists = await _context.Students.AsNoTracking().AnyAsync(x => x.Id == studentId && !x.IsDeleted);
            if (!studentExists) return NotFound();

            var enrollments = await (from enrollment in _context.Enrollments.AsNoTracking()
                                     join course in _context.Courses.AsNoTracking() on enrollment.CourseId equals course.Id into courseJoin
                                     from course in courseJoin.DefaultIfEmpty()
                                     join semester in _context.Semesters.AsNoTracking() on enrollment.SemesterId equals semester.Id into semesterJoin
                                     from semester in semesterJoin.DefaultIfEmpty()
                                     where enrollment.StudentId == studentId && !enrollment.IsDeleted
                                     select new StudentEnrollmentResponse
                                     {
                                         EnrollmentId = enrollment.Id,
                                         StudentId = enrollment.StudentId,
                                         CourseId = enrollment.CourseId,
                                         CourseCode = course == null ? null : course.CourseCode,
                                         CourseName = course == null ? null : course.CourseName,
                                         Units = course == null ? null : (int?)course.Units,
                                         SemesterId = enrollment.SemesterId,
                                         SemesterName = semester == null ? null : semester.SemesterName,
                                         SchoolYear = semester == null ? null : semester.SchoolYear,
                                         Status = enrollment.Status,
                                         CurrentGrade = _context.Grades.AsNoTracking()
                                             .Where(g => g.StudentId == enrollment.StudentId
                                                         && g.CourseId == enrollment.CourseId
                                                         && g.SemesterId == enrollment.SemesterId
                                                         && !g.IsDeleted)
                                             .OrderByDescending(g => g.CreatedAt)
                                             .Select(g => g.GradeValue)
                                             .FirstOrDefault()
                                     })
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpGet("{studentId:int}/enrollments/{enrollmentId:int}")]
        [ProducesResponseType(typeof(StudentEnrollmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentEnrollmentResponse>> GetStudentEnrollmentById(int studentId, int enrollmentId)
        {
            var enrollment = await (from item in _context.Enrollments.AsNoTracking()
                                    join course in _context.Courses.AsNoTracking() on item.CourseId equals course.Id into courseJoin
                                    from course in courseJoin.DefaultIfEmpty()
                                    join semester in _context.Semesters.AsNoTracking() on item.SemesterId equals semester.Id into semesterJoin
                                    from semester in semesterJoin.DefaultIfEmpty()
                                    where item.Id == enrollmentId && item.StudentId == studentId && !item.IsDeleted
                                    select new StudentEnrollmentResponse
                                    {
                                        EnrollmentId = item.Id,
                                        StudentId = item.StudentId,
                                        CourseId = item.CourseId,
                                        CourseCode = course == null ? null : course.CourseCode,
                                        CourseName = course == null ? null : course.CourseName,
                                        Units = course == null ? null : (int?)course.Units,
                                        SemesterId = item.SemesterId,
                                        SemesterName = semester == null ? null : semester.SemesterName,
                                        SchoolYear = semester == null ? null : semester.SchoolYear,
                                        Status = item.Status,
                                        CurrentGrade = _context.Grades.AsNoTracking()
                                            .Where(g => g.StudentId == item.StudentId
                                                        && g.CourseId == item.CourseId
                                                        && g.SemesterId == item.SemesterId
                                                        && !g.IsDeleted)
                                            .OrderByDescending(g => g.CreatedAt)
                                            .Select(g => g.GradeValue)
                                            .FirstOrDefault()
                                    })
                .FirstOrDefaultAsync();

            if (enrollment == null) return NotFound();
            return Ok(enrollment);
        }

        [HttpPost("{studentId:int}/enrollments")]
        public async Task<ActionResult<Enrollment>> CreateStudentEnrollment(int studentId, [FromBody] Enrollment enrollment)
        {
            var studentExists = await _context.Students.AsNoTracking().AnyAsync(x => x.Id == studentId && !x.IsDeleted);
            if (!studentExists) return NotFound();

            enrollment.StudentId = studentId;
            enrollment.IsDeleted = false;
            enrollment.DeleteDate = null;
            enrollment.DeleteName = null;
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudentEnrollmentById), new { studentId, enrollmentId = enrollment.Id }, enrollment);
        }

        [HttpPut("{studentId:int}/enrollments/{enrollmentId:int}")]
        public async Task<IActionResult> UpdateStudentEnrollment(int studentId, int enrollmentId, [FromBody] Enrollment enrollment)
        {
            var existing = await _context.Enrollments.FirstOrDefaultAsync(x => x.Id == enrollmentId && x.StudentId == studentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.CourseId = enrollment.CourseId;
            existing.SemesterId = enrollment.SemesterId;
            existing.Status = enrollment.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{studentId:int}/enrollments/{enrollmentId:int}")]
        public async Task<IActionResult> DeleteStudentEnrollment(int studentId, int enrollmentId)
        {
            var existing = await _context.Enrollments.FirstOrDefaultAsync(x => x.Id == enrollmentId && x.StudentId == studentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.IsDeleted = true;
            existing.DeleteDate = DateTime.Now;
            existing.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{studentId:int}/grades")]
        [ProducesResponseType(typeof(IEnumerable<StudentGradeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<StudentGradeResponse>>> GetStudentGrades(int studentId)
        {
            var studentExists = await _context.Students.AsNoTracking().AnyAsync(x => x.Id == studentId && !x.IsDeleted);
            if (!studentExists) return NotFound();

            var grades = await (from grade in _context.Grades.AsNoTracking()
                                join course in _context.Courses.AsNoTracking() on grade.CourseId equals course.Id into courseJoin
                                from course in courseJoin.DefaultIfEmpty()
                                join semester in _context.Semesters.AsNoTracking() on grade.SemesterId equals semester.Id into semesterJoin
                                from semester in semesterJoin.DefaultIfEmpty()
                                where grade.StudentId == studentId && !grade.IsDeleted
                                select new StudentGradeResponse
                                {
                                    GradeId = grade.Id,
                                    StudentId = grade.StudentId,
                                    CourseId = grade.CourseId,
                                    CourseCode = course == null ? null : course.CourseCode,
                                    CourseName = course == null ? null : course.CourseName,
                                    SemesterId = grade.SemesterId,
                                    SemesterName = semester == null ? null : semester.SemesterName,
                                    SchoolYear = semester == null ? null : semester.SchoolYear,
                                    GradeValue = grade.GradeValue,
                                    CurrentGrade = grade.GradeValue,
                                    Units = grade.Units,
                                    NumberOfTakes = grade.NumberOfTakes
                                })
                .ToListAsync();

            return Ok(grades);
        }

        [HttpGet("{studentId:int}/grades/{gradeId:int}")]
        [ProducesResponseType(typeof(StudentGradeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentGradeResponse>> GetStudentGradeById(int studentId, int gradeId)
        {
            var grade = await (from item in _context.Grades.AsNoTracking()
                               join course in _context.Courses.AsNoTracking() on item.CourseId equals course.Id into courseJoin
                               from course in courseJoin.DefaultIfEmpty()
                               join semester in _context.Semesters.AsNoTracking() on item.SemesterId equals semester.Id into semesterJoin
                               from semester in semesterJoin.DefaultIfEmpty()
                               where item.Id == gradeId && item.StudentId == studentId && !item.IsDeleted
                               select new StudentGradeResponse
                               {
                                   GradeId = item.Id,
                                   StudentId = item.StudentId,
                                   CourseId = item.CourseId,
                                   CourseCode = course == null ? null : course.CourseCode,
                                   CourseName = course == null ? null : course.CourseName,
                                   SemesterId = item.SemesterId,
                                   SemesterName = semester == null ? null : semester.SemesterName,
                                   SchoolYear = semester == null ? null : semester.SchoolYear,
                                   GradeValue = item.GradeValue,
                                   CurrentGrade = item.GradeValue,
                                   Units = item.Units,
                                   NumberOfTakes = item.NumberOfTakes
                               })
                .FirstOrDefaultAsync();

            if (grade == null) return NotFound();
            return Ok(grade);
        }

        [HttpPost("{studentId:int}/grades")]
        public async Task<ActionResult<Grade>> CreateStudentGrade(int studentId, [FromBody] Grade grade)
        {
            var studentExists = await _context.Students.AsNoTracking().AnyAsync(x => x.Id == studentId && !x.IsDeleted);
            if (!studentExists) return NotFound();

            grade.StudentId = studentId;
            grade.IsDeleted = false;
            grade.DeleteDate = null;
            grade.DeleteName = null;
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            await PublishFailedGradeReminderAsync(grade);

            return CreatedAtAction(nameof(GetStudentGradeById), new { studentId, gradeId = grade.Id }, grade);
        }

        [HttpPut("{studentId:int}/grades/{gradeId:int}")]
        public async Task<IActionResult> UpdateStudentGrade(int studentId, int gradeId, [FromBody] Grade grade)
        {
            var existing = await _context.Grades.FirstOrDefaultAsync(x => x.Id == gradeId && x.StudentId == studentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.CourseId = grade.CourseId;
            existing.SemesterId = grade.SemesterId;
            existing.GradeValue = grade.GradeValue;
            existing.Units = grade.Units;
            existing.NumberOfTakes = grade.NumberOfTakes;
            await _context.SaveChangesAsync();

            await PublishFailedGradeReminderAsync(existing);
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{studentId:int}/grades/{gradeId:int}")]
        public async Task<IActionResult> DeleteStudentGrade(int studentId, int gradeId)
        {
            var existing = await _context.Grades.FirstOrDefaultAsync(x => x.Id == gradeId && x.StudentId == studentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.IsDeleted = true;
            existing.DeleteDate = DateTime.Now;
            existing.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task PublishFailedGradeReminderAsync(Grade grade)
        {
            if (!grade.GradeValue.HasValue || grade.GradeValue.Value < 5m)
            {
                return;
            }

            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == grade.StudentId && !x.IsDeleted);
            if (student == null || !student.YearLevelId.HasValue)
            {
                return;
            }

            var studentName = await _context.Users.AsNoTracking()
                .Where(x => x.Id == student.UserId && x.IsActive)
                .Select(x => string.IsNullOrWhiteSpace((x.FirstName + " " + x.LastName).Trim())
                    ? x.Username
                    : (x.FirstName + " " + x.LastName).Trim())
                .FirstOrDefaultAsync();

            await _notificationStreamManager.PublishAsync(
                new NotificationPayload
                {
                    Id = grade.Id.ToString(),
                    Type = "warning",
                    Title = "Failing grade recorded",
                    Message = string.IsNullOrWhiteSpace(studentName)
                        ? "A failing grade was recorded."
                        : $"A failing grade was recorded for {studentName}.",
                    CreatedAt = DateTime.UtcNow,
                    Action = new NotificationActionResponse
                    {
                        Kind = "students"
                    }
                },
                new NotificationAudience { YearLevelId = student.YearLevelId.Value });
        }

        private async Task<List<int>> GetAssignedYearLevelIdsForCurrentAdviserAsync()
        {
            var username = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User?.Identity?.Name
                           ?? User?.FindFirst("UserName")?.Value;

            if (string.IsNullOrWhiteSpace(username))
            {
                return new List<int>();
            }

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
            if (user == null)
            {
                return new List<int>();
            }

            var adviser = await _context.Advisers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted);
            if (adviser == null)
            {
                return new List<int>();
            }

            return await _context.AdviserAssignments.AsNoTracking()
                .Where(x => x.AdviserId == adviser.Id && !x.IsDeleted)
                .Select(x => x.YearLevelId)
                .Distinct()
                .ToListAsync();
        }

        private static bool IsAllowedAvailabilityDay(string dayOfWeek)
        {
            return !string.IsNullOrWhiteSpace(dayOfWeek)
                && AllowedAvailabilityDays.Contains(dayOfWeek.Trim());
        }

        private static string NormalizeDayOfWeek(string dayOfWeek)
        {
            if (string.IsNullOrWhiteSpace(dayOfWeek)) return null;

            var normalized = dayOfWeek.Trim().ToUpperInvariant();
            return normalized switch
            {
                "MONDAY" => "Monday",
                "TUESDAY" => "Tuesday",
                "WEDNESDAY" => "Wednesday",
                "THURSDAY" => "Thursday",
                "FRIDAY" => "Friday",
                "SATURDAY" => "Saturday",
                _ => dayOfWeek.Trim()
            };
        }

        private static List<string> BuildThirtyMinuteSlots(TimeSpan? startTime, TimeSpan? endTime)
        {
            var slots = new List<string>();

            if (!startTime.HasValue || !endTime.HasValue || endTime <= startTime)
            {
                return slots;
            }

            var cursor = startTime.Value;
            while (cursor.Add(TimeSpan.FromMinutes(AppointmentSessionMinutes)) <= endTime.Value)
            {
                slots.Add(cursor.ToString(@"hh\:mm"));
                cursor = cursor.Add(TimeSpan.FromMinutes(AppointmentSessionMinutes));
            }

            return slots;
        }

        private async Task<Student> GetCurrentStudentAsync()
        {
            var username = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User?.Identity?.Name
                           ?? User?.FindFirst("UserName")?.Value;

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == username && x.IsActive && x.Role == "STUDENT");
            if (user == null)
            {
                return null;
            }

            return await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted);
        }

        private async Task<List<StudentAssignedAdviserResponse>> GetAssignedAdvisersByYearLevelAsync(int yearLevelId)
        {
            return await (
                from assignment in _context.AdviserAssignments.AsNoTracking()
                join adviser in _context.Advisers.AsNoTracking() on assignment.AdviserId equals adviser.Id
                join user in _context.Users.AsNoTracking() on adviser.UserId equals user.Id
                join yearLevel in _context.YearLevels.AsNoTracking() on assignment.YearLevelId equals yearLevel.Id into yearLevelJoin
                from yearLevel in yearLevelJoin.DefaultIfEmpty()
                where !assignment.IsDeleted
                      && !adviser.IsDeleted
                      && user.IsActive
                      && assignment.YearLevelId == yearLevelId
                select new StudentAssignedAdviserResponse
                {
                    AdviserId = adviser.Id,
                    UserId = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = string.IsNullOrWhiteSpace((user.FirstName + " " + user.LastName).Trim())
                        ? user.Username
                        : (user.FirstName + " " + user.LastName).Trim(),
                    Email = user.Email,
                    YearLevelId = assignment.YearLevelId,
                    YearLevelName = yearLevel == null ? null : yearLevel.YearName
                })
                .Distinct()
                .ToListAsync();
        }

        [HttpGet("year-levels")]
        public async Task<ActionResult<IEnumerable<YearLevel>>> GetYearLevels()
        {
            return Ok(await _context.YearLevels.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("year-levels/{yearLevelId:int}")]
        public async Task<ActionResult<YearLevel>> GetYearLevelById(int yearLevelId)
        {
            var yearLevel = await _context.YearLevels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == yearLevelId && !x.IsDeleted);
            if (yearLevel == null) return NotFound();
            return Ok(yearLevel);
        }

        [HttpGet("{studentId:int}/year-level")]
        public async Task<ActionResult<YearLevel>> GetStudentYearLevel(int studentId)
        {
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);
            if (student == null || !student.YearLevelId.HasValue) return NotFound();

            var yearLevel = await _context.YearLevels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == student.YearLevelId.Value && !x.IsDeleted);
            if (yearLevel == null) return NotFound();
            return Ok(yearLevel);
        }

        [HttpPost("year-levels")]
        public async Task<ActionResult<YearLevel>> CreateYearLevel([FromBody] YearLevel yearLevel)
        {
            yearLevel.IsDeleted = false;
            yearLevel.DeleteDate = null;
            yearLevel.DeleteName = null;
            _context.YearLevels.Add(yearLevel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetYearLevelById), new { yearLevelId = yearLevel.Id }, yearLevel);
        }

        [HttpPut("year-levels/{yearLevelId:int}")]
        public async Task<IActionResult> UpdateYearLevel(int yearLevelId, [FromBody] YearLevel yearLevel)
        {
            if (yearLevelId != yearLevel.Id) return BadRequest();
            var existing = await _context.YearLevels.FirstOrDefaultAsync(x => x.Id == yearLevelId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.YearName = yearLevel.YearName;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("year-levels/{yearLevelId:int}")]
        public async Task<IActionResult> DeleteYearLevel(int yearLevelId)
        {
            var yearLevel = await _context.YearLevels.FirstOrDefaultAsync(x => x.Id == yearLevelId && !x.IsDeleted);
            if (yearLevel == null) return NotFound();

            yearLevel.IsDeleted = true;
            yearLevel.DeleteDate = DateTime.Now;
            yearLevel.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
