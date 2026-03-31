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
    [Route("api")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdviserAvailabilitiesController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AdviserAvailabilitiesController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet("advisers/{adviserId:int}/availabilities")]
        public async Task<ActionResult<IEnumerable<AdviserAvailability>>> GetAdviserAvailabilities(int adviserId)
        {
            var items = await _context.AdviserAvailabilities
                .AsNoTracking()
                .Where(x => x.AdviserId == adviserId && !x.IsDeleted)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("adviser-availabilities/{availabilityId:int}")]
        public async Task<ActionResult<AdviserAvailability>> GetAdviserAvailabilityById(int availabilityId)
        {
            var item = await _context.AdviserAvailabilities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == availabilityId && !x.IsDeleted);

            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("advisers/{adviserId:int}/availabilities/day/{dayOfWeek}")]
        public async Task<ActionResult<IEnumerable<AdviserAvailability>>> GetAdviserAvailabilitiesByDay(int adviserId, string dayOfWeek)
        {
            var items = await _context.AdviserAvailabilities
                .AsNoTracking()
                .Where(x => x.AdviserId == adviserId && x.DayOfWeek == dayOfWeek && !x.IsDeleted)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost("advisers/{adviserId:int}/availabilities")]
        public async Task<ActionResult<AdviserAvailability>> CreateAdviserAvailability(int adviserId, [FromBody] AdviserAvailability item)
        {
            item.AdviserId = adviserId;
            item.IsDeleted = false;
            item.DeleteDate = null;
            item.DeleteName = null;

            _context.AdviserAvailabilities.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAdviserAvailabilityById), new { availabilityId = item.Id }, item);
        }

        [HttpPut("adviser-availabilities/{availabilityId:int}")]
        public async Task<IActionResult> UpdateAdviserAvailability(int availabilityId, [FromBody] AdviserAvailability item)
        {
            if (availabilityId != item.Id) return BadRequest();

            var existing = await _context.AdviserAvailabilities.FirstOrDefaultAsync(x => x.Id == availabilityId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.AdviserId = item.AdviserId;
            existing.DayOfWeek = item.DayOfWeek;
            existing.StartTime = item.StartTime;
            existing.EndTime = item.EndTime;
            existing.Location = item.Location;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("adviser-availabilities/{availabilityId:int}")]
        public async Task<IActionResult> DeleteAdviserAvailability(int availabilityId)
        {
            var item = await _context.AdviserAvailabilities.FirstOrDefaultAsync(x => x.Id == availabilityId && !x.IsDeleted);
            if (item == null) return NotFound();

            item.IsDeleted = true;
            item.DeleteDate = DateTime.Now;
            item.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
