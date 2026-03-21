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
    public class AdviserAssignmentsController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AdviserAssignmentsController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdviserAssignment>>> GetAll()
        {
            return Ok(await _context.AdviserAssignments.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdviserAssignment>> GetById(int id)
        {
            var assignment = await _context.AdviserAssignments.FindAsync(id);
            if (assignment == null) return NotFound();
            return Ok(assignment);
        }

        [HttpPost]
        public async Task<ActionResult<AdviserAssignment>> Create([FromBody] AdviserAssignment assignment)
        {
            _context.AdviserAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdviserAssignment assignment)
        {
            if (id != assignment.Id) return BadRequest();
            if (!await _context.AdviserAssignments.AnyAsync(x => x.Id == id)) return NotFound();

            _context.Entry(assignment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.AdviserAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            _context.AdviserAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
