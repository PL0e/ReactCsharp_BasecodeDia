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
    public class GradesController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public GradesController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Grade>>> GetAll()
        {
            return Ok(await _context.Grades.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Grade>> GetById(int id)
        {
            var grade = await _context.Grades.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (grade == null) return NotFound();
            return Ok(grade);
        }

        [HttpPost]
        public async Task<ActionResult<Grade>> Create([FromBody] Grade grade)
        {
            grade.IsDeleted = false;
            grade.DeleteDate = null;
            grade.DeleteName = null;
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = grade.Id }, grade);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Grade grade)
        {
            if (id != grade.Id) return BadRequest();
            var existing = await _context.Grades.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.StudentId = grade.StudentId;
            existing.CourseId = grade.CourseId;
            existing.SemesterId = grade.SemesterId;
            existing.GradeValue = grade.GradeValue;
            existing.Units = grade.Units;
            existing.NumberOfTakes = grade.NumberOfTakes;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var grade = await _context.Grades.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (grade == null) return NotFound();

            grade.IsDeleted = true;
            grade.DeleteDate = DateTime.Now;
            grade.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
