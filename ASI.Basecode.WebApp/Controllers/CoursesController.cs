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
    public class CoursesController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public CoursesController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAll()
        {
            return Ok(await _context.Courses.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Course>> GetById(int id)
        {
            var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (course == null) return NotFound();
            return Ok(course);
        }

        [HttpPost]
        public async Task<ActionResult<Course>> Create([FromBody] Course course)
        {
            course.IsDeleted = false;
            course.DeleteDate = null;
            course.DeleteName = null;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Course course)
        {
            if (id != course.Id) return BadRequest();
            var existing = await _context.Courses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.CourseCode = course.CourseCode;
            existing.CourseName = course.CourseName;
            existing.Units = course.Units;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (course == null) return NotFound();

            course.IsDeleted = true;
            course.DeleteDate = DateTime.Now;
            course.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
