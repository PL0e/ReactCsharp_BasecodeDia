using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Api;
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
    public class AppointmentsController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AppointmentsController(AsiBasecodeDBContext context)
        {
            _context = context;
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
        public async Task<ActionResult<AppointmentResponse>> Create([FromBody] UpsertAppointmentRequest request)
        {
            var appointment = new Appointment
            {
                StudentId = request.StudentId,
                AdviserId = request.AdviserId,
                SemesterId = request.SemesterId,
                AppointmentType = request.AppointmentType,
                AppointmentDate = request.AppointmentDate,
                AppointmentTime = request.AppointmentTime,
                Status = request.Status,
                CancellationReason = request.CancellationReason,
                CancellationDate = request.CancellationDate,
                IsDeleted = false,
                DeleteDate = null,
                DeleteName = null
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

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

            return CreatedAtAction(nameof(GetById), new { appointmentId = appointment.Id }, response);
        }

        [HttpPut("{appointmentId:int}")]
        public async Task<IActionResult> Update(int appointmentId, [FromBody] UpsertAppointmentRequest request)
        {
            var existing = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == appointmentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.StudentId = request.StudentId;
            existing.AdviserId = request.AdviserId;
            existing.SemesterId = request.SemesterId;
            existing.AppointmentType = request.AppointmentType;
            existing.AppointmentDate = request.AppointmentDate;
            existing.AppointmentTime = request.AppointmentTime;
            existing.Status = request.Status;
            existing.CancellationReason = request.CancellationReason;
            existing.CancellationDate = request.CancellationDate;
            await _context.SaveChangesAsync();
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
