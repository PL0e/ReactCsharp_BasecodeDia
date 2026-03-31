using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
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
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAll()
        {
            return Ok(await _context.Appointments.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Appointment>> GetById(int id)
        {
            var appointment = await _context.Appointments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> Create([FromBody] Appointment appointment)
        {
            appointment.IsDeleted = false;
            appointment.DeleteDate = null;
            appointment.DeleteName = null;
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Appointment appointment)
        {
            if (id != appointment.Id) return BadRequest();
            var existing = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.StudentId = appointment.StudentId;
            existing.AdviserId = appointment.AdviserId;
            existing.SemesterId = appointment.SemesterId;
            existing.AppointmentType = appointment.AppointmentType;
            existing.AppointmentDate = appointment.AppointmentDate;
            existing.AppointmentTime = appointment.AppointmentTime;
            existing.Status = appointment.Status;
            existing.CancellationReason = appointment.CancellationReason;
            existing.CancellationDate = appointment.CancellationDate;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (appointment == null) return NotFound();

            appointment.IsDeleted = true;
            appointment.DeleteDate = DateTime.Now;
            appointment.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
