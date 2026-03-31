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
    public class SemestersController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public SemestersController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Semester>>> GetAll()
        {
            return Ok(await _context.Semesters.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Semester>> GetById(int id)
        {
            var semester = await _context.Semesters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (semester == null) return NotFound();
            return Ok(semester);
        }

        [HttpPost]
        public async Task<ActionResult<Semester>> Create([FromBody] Semester semester)
        {
            semester.IsDeleted = false;
            semester.DeleteDate = null;
            semester.DeleteName = null;
            _context.Semesters.Add(semester);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = semester.Id }, semester);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Semester semester)
        {
            if (id != semester.Id) return BadRequest();
            var existing = await _context.Semesters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.SemesterName = semester.SemesterName;
            existing.SchoolYear = semester.SchoolYear;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var semester = await _context.Semesters.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (semester == null) return NotFound();

            semester.IsDeleted = true;
            semester.DeleteDate = DateTime.Now;
            semester.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
