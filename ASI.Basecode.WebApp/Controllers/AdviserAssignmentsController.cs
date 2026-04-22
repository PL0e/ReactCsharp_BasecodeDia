using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult<IEnumerable<AdviserAssignmentResponse>>> GetAll()
        {
            var assignments = await _context.AdviserAssignments.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new AdviserAssignmentResponse
                {
                    AdviserAssignmentId = x.Id,
                    AdviserId = x.AdviserId,
                    YearLevelId = x.YearLevelId,
                    AssignedAt = x.AssignedAt,
                    IsDeleted = x.IsDeleted,
                    DeleteDate = x.DeleteDate,
                    DeleteName = x.DeleteName
                })
                .ToListAsync();

            return Ok(assignments);
        }

        [HttpGet("{adviserAssignmentId:int}")]
        public async Task<ActionResult<AdviserAssignmentResponse>> GetById(int adviserAssignmentId)
        {
            var assignment = await _context.AdviserAssignments.AsNoTracking()
                .Where(x => x.Id == adviserAssignmentId && !x.IsDeleted)
                .Select(x => new AdviserAssignmentResponse
                {
                    AdviserAssignmentId = x.Id,
                    AdviserId = x.AdviserId,
                    YearLevelId = x.YearLevelId,
                    AssignedAt = x.AssignedAt,
                    IsDeleted = x.IsDeleted,
                    DeleteDate = x.DeleteDate,
                    DeleteName = x.DeleteName
                })
                .FirstOrDefaultAsync();

            if (assignment == null) return NotFound();
            return Ok(assignment);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AdviserAssignmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AdviserAssignmentResponse>> Create([FromBody] UpsertAdviserAssignmentRequest request)
        {
            var assignedAt = request.AssignedAt < new DateTime(1753, 1, 1)
                ? DateTime.Now
                : request.AssignedAt;

            var adviserExists = await _context.Advisers.AsNoTracking()
                .AnyAsync(x => x.Id == request.AdviserId);
            if (!adviserExists)
            {
                return BadRequest(new MessageResponse { Message = "Invalid adviserId." });
            }

            var yearLevelExists = await _context.YearLevels.AsNoTracking()
                .AnyAsync(x => x.Id == request.YearLevelId);
            if (!yearLevelExists)
            {
                return BadRequest(new MessageResponse { Message = "Invalid yearLevelId." });
            }

            var assignment = new AdviserAssignment
            {
                AdviserId = request.AdviserId,
                YearLevelId = request.YearLevelId,
                AssignedAt = assignedAt,
                IsDeleted = false,
                DeleteDate = null,
                DeleteName = null
            };

            _context.AdviserAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var response = new AdviserAssignmentResponse
            {
                AdviserAssignmentId = assignment.Id,
                AdviserId = assignment.AdviserId,
                YearLevelId = assignment.YearLevelId,
                AssignedAt = assignment.AssignedAt,
                IsDeleted = assignment.IsDeleted,
                DeleteDate = assignment.DeleteDate,
                DeleteName = assignment.DeleteName
            };

            return CreatedAtAction(nameof(GetById), new { adviserAssignmentId = assignment.Id }, response);
        }

        [HttpPut("{adviserAssignmentId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int adviserAssignmentId, [FromBody] UpsertAdviserAssignmentRequest request)
        {
            var assignedAt = request.AssignedAt < new DateTime(1753, 1, 1)
                ? DateTime.Now
                : request.AssignedAt;

            var existing = await _context.AdviserAssignments.FirstOrDefaultAsync(x => x.Id == adviserAssignmentId && !x.IsDeleted);
            if (existing == null) return NotFound();

            var adviserExists = await _context.Advisers.AsNoTracking()
                .AnyAsync(x => x.Id == request.AdviserId);
            if (!adviserExists)
            {
                return BadRequest(new MessageResponse { Message = "Invalid adviserId." });
            }

            var yearLevelExists = await _context.YearLevels.AsNoTracking()
                .AnyAsync(x => x.Id == request.YearLevelId);
            if (!yearLevelExists)
            {
                return BadRequest(new MessageResponse { Message = "Invalid yearLevelId." });
            }

            existing.AdviserId = request.AdviserId;
            existing.YearLevelId = request.YearLevelId;
            existing.AssignedAt = assignedAt;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{adviserAssignmentId:int}/reassign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reassign(int adviserAssignmentId, [FromBody] UpsertAdviserAssignmentRequest request)
        {
            return await Update(adviserAssignmentId, request);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{adviserAssignmentId:int}")]
        public async Task<IActionResult> Delete(int adviserAssignmentId)
        {
            var assignment = await _context.AdviserAssignments.FirstOrDefaultAsync(x => x.Id == adviserAssignmentId && !x.IsDeleted);
            if (assignment == null) return NotFound();

            assignment.IsDeleted = true;
            assignment.DeleteDate = DateTime.Now;
            assignment.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
