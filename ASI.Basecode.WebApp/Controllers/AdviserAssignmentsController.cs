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
            return Ok(await _context.AdviserAssignments.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdviserAssignment>> GetById(int id)
        {
            var assignment = await _context.AdviserAssignments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (assignment == null) return NotFound();
            return Ok(assignment);
        }

        [HttpPost]
        public async Task<ActionResult<AdviserAssignment>> Create([FromBody] AdviserAssignment assignment)
        {
            assignment.IsDeleted = false;
            assignment.DeleteDate = null;
            assignment.DeleteName = null;
            _context.AdviserAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdviserAssignment assignment)
        {
            if (id != assignment.Id) return BadRequest();
            var existing = await _context.AdviserAssignments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.AdviserId = assignment.AdviserId;
            existing.YearLevelId = assignment.YearLevelId;
            existing.AssignedAt = assignment.AssignedAt;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.AdviserAssignments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (assignment == null) return NotFound();

            assignment.IsDeleted = true;
            assignment.DeleteDate = DateTime.Now;
            assignment.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
