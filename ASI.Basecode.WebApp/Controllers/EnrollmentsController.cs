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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class EnrollmentsController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public EnrollmentsController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetAll()
        {
            return Ok(await _context.Enrollments.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{enrollmentId:int}")]
        public async Task<ActionResult<Enrollment>> GetById(int enrollmentId)
        {
            var item = await _context.Enrollments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDeleted);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<Enrollment>> Create([FromBody] Enrollment item)
        {
            item.IsDeleted = false;
            item.DeleteDate = null;
            item.DeleteName = null;
            _context.Enrollments.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { enrollmentId = item.Id }, item);
        }

        [HttpPut("{enrollmentId:int}")]
        public async Task<IActionResult> Update(int enrollmentId, [FromBody] Enrollment item)
        {
            if (enrollmentId != item.Id) return BadRequest();
            var existing = await _context.Enrollments.FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.StudentId = item.StudentId;
            existing.CourseId = item.CourseId;
            existing.SemesterId = item.SemesterId;
            existing.Status = item.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{enrollmentId:int}")]
        public async Task<IActionResult> Delete(int enrollmentId)
        {
            var item = await _context.Enrollments.FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDeleted);
            if (item == null) return NotFound();

            item.IsDeleted = true;
            item.DeleteDate = DateTime.Now;
            item.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
