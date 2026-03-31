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
    public class StudentsController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public StudentsController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetAll()
        {
            return Ok(await _context.Students.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Student>> GetById(int id)
        {
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpPost]
        public async Task<ActionResult<Student>> Create([FromBody] Student student)
        {
            student.IsDeleted = false;
            student.DeleteDate = null;
            student.DeleteName = null;
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Student student)
        {
            if (id != student.Id) return BadRequest();
            var existing = await _context.Students.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.UserId = student.UserId;
            existing.YearLevelId = student.YearLevelId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (student == null) return NotFound();

            student.IsDeleted = true;
            student.DeleteDate = DateTime.Now;
            student.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
