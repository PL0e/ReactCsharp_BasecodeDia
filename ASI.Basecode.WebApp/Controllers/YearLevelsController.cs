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
    public class YearLevelsController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public YearLevelsController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<YearLevel>>> GetAll()
        {
            return Ok(await _context.YearLevels.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<YearLevel>> GetById(int id)
        {
            var yearLevel = await _context.YearLevels.FindAsync(id);
            if (yearLevel == null) return NotFound();
            return Ok(yearLevel);
        }

        [HttpPost]
        public async Task<ActionResult<YearLevel>> Create([FromBody] YearLevel yearLevel)
        {
            _context.YearLevels.Add(yearLevel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = yearLevel.Id }, yearLevel);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] YearLevel yearLevel)
        {
            if (id != yearLevel.Id) return BadRequest();
            if (!await _context.YearLevels.AnyAsync(x => x.Id == id)) return NotFound();

            _context.Entry(yearLevel).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var yearLevel = await _context.YearLevels.FindAsync(id);
            if (yearLevel == null) return NotFound();

            _context.YearLevels.Remove(yearLevel);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
