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
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AppointmentsController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;
        private readonly NotificationStreamManager _notificationStreamManager;

        public AppointmentsController(AsiBasecodeDBContext context, NotificationStreamManager notificationStreamManager)
        {
            _context = context;
            _notificationStreamManager = notificationStreamManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAll()
        {
            var appointments = await _context.Appointments.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new AppointmentResponse
                {
                    AppointmentId = x.Id,
                    StudentId = x.StudentId,
                    AdviserId = x.AdviserId,
                    SemesterId = x.SemesterId,
                    AppointmentType = x.AppointmentType,
                    AppointmentDate = x.AppointmentDate,
                    AppointmentTime = x.AppointmentTime,
                    Status = x.Status,
                    CancellationReason = x.CancellationReason,
                    CancellationDate = x.CancellationDate,
                    CreatedAt = x.CreatedAt,
                    IsDeleted = x.IsDeleted,
                    DeleteDate = x.DeleteDate,
                    DeleteName = x.DeleteName
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("calendar")]
        [ProducesResponseType(typeof(AppointmentCalendarResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppointmentCalendarResponse>> GetCalendar()
        {
            var currentRole = User?.FindFirst(ClaimTypes.Role)?.Value?.ToUpperInvariant();
            var isAdviserScoped = currentRole == "ADVISER" || currentRole == "CHAIRMAN";

            var scopedYearLevelIds = new List<int>();
            if (isAdviserScoped)
            {
                scopedYearLevelIds = await GetAssignedYearLevelIdsForCurrentAdviserAsync();
                if (scopedYearLevelIds.Count == 0)
                {
                    return Ok(new AppointmentCalendarResponse());
                }
            }

            var items = await (
                from appointment in _context.Appointments.AsNoTracking()
                join student in _context.Students.AsNoTracking() on appointment.StudentId equals student.Id into studentJoin
                from student in studentJoin.DefaultIfEmpty()
                join studentUser in _context.Users.AsNoTracking() on student.UserId equals (int?)studentUser.Id into studentUserJoin
                from studentUser in studentUserJoin.DefaultIfEmpty()
                join adviser in _context.Advisers.AsNoTracking() on appointment.AdviserId equals adviser.Id into adviserJoin
                from adviser in adviserJoin.DefaultIfEmpty()
                join adviserUser in _context.Users.AsNoTracking() on adviser.UserId equals (int?)adviserUser.Id into adviserUserJoin
                from adviserUser in adviserUserJoin.DefaultIfEmpty()
                where !appointment.IsDeleted
                      && (!isAdviserScoped || (student.YearLevelId.HasValue && scopedYearLevelIds.Contains(student.YearLevelId.Value)))
                orderby appointment.AppointmentDate, appointment.AppointmentTime
                select new AppointmentCalendarItemResponse
                {
                    AppointmentId = appointment.Id,
                    StudentId = appointment.StudentId,
                    StudentName = studentUser == null
                        ? null
                        : (string.IsNullOrWhiteSpace(studentUser.FirstName) && string.IsNullOrWhiteSpace(studentUser.LastName)
                            ? studentUser.Username
                            : ($"{studentUser.FirstName} {studentUser.LastName}".Trim())),
                    AdviserId = appointment.AdviserId,
                    AdviserName = adviserUser == null
                        ? null
                        : (string.IsNullOrWhiteSpace(adviserUser.FirstName) && string.IsNullOrWhiteSpace(adviserUser.LastName)
                            ? adviserUser.Username
                            : ($"{adviserUser.FirstName} {adviserUser.LastName}".Trim())),
                    SemesterId = appointment.SemesterId,
                    AppointmentType = appointment.AppointmentType,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    CancellationReason = appointment.CancellationReason
                })
                .ToListAsync();

            var response = new AppointmentCalendarResponse
            {
                UpcomingAppointments = items
                    .Where(x => !IsCompletedStatus(x.Status) && !IsCancelledStatus(x.Status))
                    .ToList(),
                CompletedAppointments = items
                    .Where(x => IsCompletedStatus(x.Status))
                    .ToList(),
                CancelledAppointments = items
                    .Where(x => IsCancelledStatus(x.Status))
                    .ToList()
            };

            return Ok(response);
        }

        [HttpGet("{appointmentId:int}")]
        public async Task<ActionResult<AppointmentResponse>> GetById(int appointmentId)
        {
            var appointment = await _context.Appointments.AsNoTracking()
                .Where(x => x.Id == appointmentId && !x.IsDeleted)
                .Select(x => new AppointmentResponse
                {
                    AppointmentId = x.Id,
                    StudentId = x.StudentId,
                    AdviserId = x.AdviserId,
                    SemesterId = x.SemesterId,
                    AppointmentType = x.AppointmentType,
                    AppointmentDate = x.AppointmentDate,
                    AppointmentTime = x.AppointmentTime,
                    Status = x.Status,
                    CancellationReason = x.CancellationReason,
                    CancellationDate = x.CancellationDate,
                    CreatedAt = x.CreatedAt,
                    IsDeleted = x.IsDeleted,
                    DeleteDate = x.DeleteDate,
                    DeleteName = x.DeleteName
                })
                .FirstOrDefaultAsync();

            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppointmentResponse>> Create([FromBody] UpsertAppointmentRequest request)
        {
            var currentStudent = await GetCurrentStudentAsync();
            if (currentStudent == null)
            {
                return BadRequest(new { message = "Unable to resolve the current student account for this booking." });
            }

            var resolvedSemesterId = await ResolveSemesterIdAsync(request.SemesterId, request.AppointmentDate);
            if (!resolvedSemesterId.HasValue)
            {
                return BadRequest(new { message = "SemesterId is required. Provide SemesterId or a valid AppointmentDate that falls within an active semester." });
            }

            var appointment = new Appointment
            {
                StudentId = currentStudent.Id,
                AdviserId = request.AdviserId,
                SemesterId = resolvedSemesterId.Value,
                AppointmentType = request.AppointmentType,
                AppointmentDate = request.AppointmentDate,
                AppointmentTime = request.AppointmentTime,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "UPCOMING" : request.Status,
                CancellationReason = request.CancellationReason,
                CancellationDate = request.CancellationDate,
                IsDeleted = false,
                DeleteDate = null,
                DeleteName = null
            };

            _context.Appointments.Add(appointment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Appointment could not be saved. Please verify the selected adviser, semester, and appointment date, then try again." });
            }

            var response = new AppointmentResponse
            {
                AppointmentId = appointment.Id,
                StudentId = appointment.StudentId,
                AdviserId = appointment.AdviserId,
                SemesterId = appointment.SemesterId,
                AppointmentType = appointment.AppointmentType,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Status = appointment.Status,
                CancellationReason = appointment.CancellationReason,
                CancellationDate = appointment.CancellationDate,
                CreatedAt = appointment.CreatedAt,
                IsDeleted = appointment.IsDeleted,
                DeleteDate = appointment.DeleteDate,
                DeleteName = appointment.DeleteName
            };

            var studentName = await GetStudentNameAsync(appointment.StudentId);
            await _notificationStreamManager.PublishAsync(
                BuildNotificationPayload(
                    appointment.Id.ToString(),
                    "success",
                    "Appointment booked",
                    string.IsNullOrWhiteSpace(studentName)
                        ? "Appointment booked."
                        : $"Appointment booked by {studentName}.",
                    "student-booking",
                    appointment.AppointmentDate,
                    appointment.AppointmentTime),
                new NotificationAudience { AdviserId = appointment.AdviserId });

            return CreatedAtAction(nameof(GetById), new { appointmentId = appointment.Id }, response);
        }

        [HttpPut("{appointmentId:int}")]
        public async Task<IActionResult> Update(int appointmentId, [FromBody] UpsertAppointmentRequest request)
        {
            var existing = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == appointmentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            var previousStatus = existing.Status;

            existing.StudentId = request.StudentId > 0 ? request.StudentId : existing.StudentId;
            existing.AdviserId = request.AdviserId > 0 ? request.AdviserId : existing.AdviserId;
            existing.SemesterId = request.SemesterId > 0 ? request.SemesterId : existing.SemesterId;
            existing.AppointmentType = string.IsNullOrWhiteSpace(request.AppointmentType) ? existing.AppointmentType : request.AppointmentType;
            existing.AppointmentDate = request.AppointmentDate ?? existing.AppointmentDate;
            existing.AppointmentTime = request.AppointmentTime ?? existing.AppointmentTime;
            existing.Status = string.IsNullOrWhiteSpace(request.Status) ? existing.Status : request.Status;
            existing.CancellationReason = request.CancellationReason ?? existing.CancellationReason;
            existing.CancellationDate = request.CancellationDate ?? existing.CancellationDate;
            await _context.SaveChangesAsync();

            if (!string.Equals(previousStatus, existing.Status, StringComparison.OrdinalIgnoreCase))
            {
                var studentName = await GetStudentNameAsync(existing.StudentId);

                if (IsCancelledStatus(existing.Status))
                {
                    await _notificationStreamManager.PublishAsync(
                        BuildNotificationPayload(
                            existing.Id.ToString(),
                            "warning",
                            "Appointment cancelled",
                            string.IsNullOrWhiteSpace(studentName)
                                ? "Appointment cancelled."
                                : $"Appointment for {studentName} was cancelled.",
                            "student-appointment",
                            existing.AppointmentDate,
                            existing.AppointmentTime),
                        new NotificationAudience { AdviserId = existing.AdviserId });
                }
                else if (string.Equals(existing.Status, "REMINDER_DUE", StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationStreamManager.PublishAsync(
                        BuildNotificationPayload(
                            existing.Id.ToString(),
                            "info",
                            "Appointment reminder",
                            string.IsNullOrWhiteSpace(studentName)
                                ? "Appointment reminder is due."
                                : $"Appointment reminder for {studentName} is due.",
                            "calendar",
                            existing.AppointmentDate,
                            existing.AppointmentTime),
                        new NotificationAudience { AdviserId = existing.AdviserId });
                }
            }
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{appointmentId:int}")]
        public async Task<IActionResult> Delete(int appointmentId)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == appointmentId && !x.IsDeleted);
            if (appointment == null) return NotFound();

            appointment.IsDeleted = true;
            appointment.DeleteDate = DateTime.Now;
            appointment.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();

            var studentName = await GetStudentNameAsync(appointment.StudentId);
            await _notificationStreamManager.PublishAsync(
                BuildNotificationPayload(
                    appointment.Id.ToString(),
                    "warning",
                    "Appointment cancelled",
                    string.IsNullOrWhiteSpace(studentName)
                        ? "Appointment cancelled."
                        : $"Appointment for {studentName} was cancelled.",
                    "student-appointment",
                    appointment.AppointmentDate,
                    appointment.AppointmentTime),
                new NotificationAudience { AdviserId = appointment.AdviserId });
            return NoContent();
        }

        private static bool IsCompletedStatus(string status)
        {
            return string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCancelledStatus(string status)
        {
            return string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "CANCELED", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<int?> ResolveSemesterIdAsync(int semesterId, DateTime? appointmentDate)
        {
            if (semesterId > 0)
            {
                return semesterId;
            }

            var semesters = await _context.Semesters.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.IsCurrent)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            if (appointmentDate.HasValue)
            {
                var matchingSemester = semesters.FirstOrDefault(x => TryParseSemesterDate(x.StartDate, out var startDate)
                                                                     && TryParseSemesterDate(x.EndDate, out var endDate)
                                                                     && startDate.Date <= appointmentDate.Value.Date
                                                                     && endDate.Date >= appointmentDate.Value.Date);
                if (matchingSemester != null)
                {
                    return matchingSemester.Id;
                }
            }

            return semesters.FirstOrDefault(x => x.IsCurrent)?.Id
                   ?? semesters.FirstOrDefault(x => x.IsActive)?.Id
                   ?? semesters.FirstOrDefault()?.Id;
        }

        private static bool TryParseSemesterDate(string value, out DateTime dateTime)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind, out dateTime)
                || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dateTime);
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

        private async Task<string> GetStudentNameAsync(int studentId)
        {
            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == studentId && !x.IsDeleted);
            if (student?.UserId == null)
            {
                return null;
            }

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == student.UserId && x.IsActive);
            if (user == null)
            {
                return null;
            }

            var fullName = (user.FirstName + " " + user.LastName).Trim();
            return string.IsNullOrWhiteSpace(fullName) ? user.Username : fullName;
        }

        private static NotificationPayload BuildNotificationPayload(
            string id,
            string type,
            string title,
            string message,
            string actionKind,
            DateTime? appointmentDate,
            TimeSpan? appointmentTime)
        {
            return new NotificationPayload
            {
                Id = id,
                Type = type,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                Action = new NotificationActionResponse
                {
                    Kind = actionKind,
                    AppointmentId = id,
                    AppointmentDate = appointmentDate?.ToString("yyyy-MM-dd"),
                    AppointmentTime = appointmentTime?.ToString(@"hh\:mm")
                }
            };
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
    }
}
