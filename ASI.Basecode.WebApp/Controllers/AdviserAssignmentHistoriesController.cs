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
    public class AdviserAssignmentHistoriesController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AdviserAssignmentHistoriesController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdviserAssignmentHistory>>> GetAll()
        {
            return Ok(await _context.AdviserAssignmentHistories.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdviserAssignmentHistory>> GetById(int id)
        {
            var history = await _context.AdviserAssignmentHistories.FindAsync(id);
            if (history == null) return NotFound();
            return Ok(history);
        }

        [HttpPost]
        public async Task<ActionResult<AdviserAssignmentHistory>> Create([FromBody] AdviserAssignmentHistory history)
        {
            _context.AdviserAssignmentHistories.Add(history);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = history.Id }, history);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdviserAssignmentHistory history)
        {
            if (id != history.Id) return BadRequest();
            if (!await _context.AdviserAssignmentHistories.AnyAsync(x => x.Id == id)) return NotFound();

            _context.Entry(history).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var history = await _context.AdviserAssignmentHistories.FindAsync(id);
            if (history == null) return NotFound();

            _context.AdviserAssignmentHistories.Remove(history);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
