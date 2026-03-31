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
            return Ok(await _context.YearLevels.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<YearLevel>> GetById(int id)
        {
            var yearLevel = await _context.YearLevels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (yearLevel == null) return NotFound();
            return Ok(yearLevel);
        }

        [HttpPost]
        public async Task<ActionResult<YearLevel>> Create([FromBody] YearLevel yearLevel)
        {
            yearLevel.IsDeleted = false;
            yearLevel.DeleteDate = null;
            yearLevel.DeleteName = null;
            _context.YearLevels.Add(yearLevel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = yearLevel.Id }, yearLevel);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] YearLevel yearLevel)
        {
            if (id != yearLevel.Id) return BadRequest();
            var existing = await _context.YearLevels.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.YearName = yearLevel.YearName;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var yearLevel = await _context.YearLevels.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (yearLevel == null) return NotFound();

            yearLevel.IsDeleted = true;
            yearLevel.DeleteDate = DateTime.Now;
            yearLevel.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
