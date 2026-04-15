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
    public class AppointmentNotesController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AppointmentNotesController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentNoteResponse>>> GetAll()
        {
            var notes = await _context.AppointmentNotes.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new AppointmentNoteResponse
                {
                    AppointmentNoteId = x.Id,
                    AppointmentId = x.AppointmentId,
                    AdviserId = x.AdviserId,
                    AdviserNotes = x.AdviserNotes,
                    CreatedAt = x.CreatedAt,
                    IsDeleted = x.IsDeleted,
                    DeleteDate = x.DeleteDate,
                    DeleteName = x.DeleteName
                })
                .ToListAsync();

            return Ok(notes);
        }

        [HttpGet("{appointmentNoteId:int}")]
        public async Task<ActionResult<AppointmentNoteResponse>> GetById(int appointmentNoteId)
        {
            var note = await _context.AppointmentNotes.AsNoTracking()
                .Where(x => x.Id == appointmentNoteId && !x.IsDeleted)
                .Select(x => new AppointmentNoteResponse
                {
                    AppointmentNoteId = x.Id,
                    AppointmentId = x.AppointmentId,
                    AdviserId = x.AdviserId,
                    AdviserNotes = x.AdviserNotes,
                    CreatedAt = x.CreatedAt,
                    IsDeleted = x.IsDeleted,
                    DeleteDate = x.DeleteDate,
                    DeleteName = x.DeleteName
                })
                .FirstOrDefaultAsync();

            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentNoteResponse>> Create([FromBody] UpsertAppointmentNoteRequest request)
        {
            var note = new AppointmentNote
            {
                AppointmentId = request.AppointmentId,
                AdviserId = request.AdviserId,
                AdviserNotes = request.AdviserNotes,
                IsDeleted = false,
                DeleteDate = null,
                DeleteName = null
            };

            _context.AppointmentNotes.Add(note);
            await _context.SaveChangesAsync();

            var response = new AppointmentNoteResponse
            {
                AppointmentNoteId = note.Id,
                AppointmentId = note.AppointmentId,
                AdviserId = note.AdviserId,
                AdviserNotes = note.AdviserNotes,
                CreatedAt = note.CreatedAt,
                IsDeleted = note.IsDeleted,
                DeleteDate = note.DeleteDate,
                DeleteName = note.DeleteName
            };

            return CreatedAtAction(nameof(GetById), new { appointmentNoteId = note.Id }, response);
        }

        [HttpPut("{appointmentNoteId:int}")]
        public async Task<IActionResult> Update(int appointmentNoteId, [FromBody] UpsertAppointmentNoteRequest request)
        {
            var existing = await _context.AppointmentNotes.FirstOrDefaultAsync(x => x.Id == appointmentNoteId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.AppointmentId = request.AppointmentId;
            existing.AdviserId = request.AdviserId;
            existing.AdviserNotes = request.AdviserNotes;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{appointmentNoteId:int}")]
        public async Task<IActionResult> Delete(int appointmentNoteId)
        {
            var note = await _context.AppointmentNotes.FirstOrDefaultAsync(x => x.Id == appointmentNoteId && !x.IsDeleted);
            if (note == null) return NotFound();

            note.IsDeleted = true;
            note.DeleteDate = DateTime.Now;
            note.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
