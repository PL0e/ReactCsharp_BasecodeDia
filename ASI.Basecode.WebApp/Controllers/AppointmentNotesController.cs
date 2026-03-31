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
    public class AppointmentNotesController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AppointmentNotesController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentNote>>> GetAll()
        {
            return Ok(await _context.AppointmentNotes.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentNote>> GetById(int id)
        {
            var note = await _context.AppointmentNotes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentNote>> Create([FromBody] AppointmentNote note)
        {
            note.IsDeleted = false;
            note.DeleteDate = null;
            note.DeleteName = null;
            _context.AppointmentNotes.Add(note);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentNote note)
        {
            if (id != note.Id) return BadRequest();
            var existing = await _context.AppointmentNotes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.AppointmentId = note.AppointmentId;
            existing.AdviserId = note.AdviserId;
            existing.AdviserNotes = note.AdviserNotes;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _context.AppointmentNotes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (note == null) return NotFound();

            note.IsDeleted = true;
            note.DeleteDate = DateTime.Now;
            note.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
