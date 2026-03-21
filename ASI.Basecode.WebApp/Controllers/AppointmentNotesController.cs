using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            return Ok(await _context.AppointmentNotes.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentNote>> GetById(int id)
        {
            var note = await _context.AppointmentNotes.FindAsync(id);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentNote>> Create([FromBody] AppointmentNote note)
        {
            _context.AppointmentNotes.Add(note);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentNote note)
        {
            if (id != note.Id) return BadRequest();
            if (!await _context.AppointmentNotes.AnyAsync(x => x.Id == id)) return NotFound();

            _context.Entry(note).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _context.AppointmentNotes.FindAsync(id);
            if (note == null) return NotFound();

            _context.AppointmentNotes.Remove(note);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
