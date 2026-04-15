using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
