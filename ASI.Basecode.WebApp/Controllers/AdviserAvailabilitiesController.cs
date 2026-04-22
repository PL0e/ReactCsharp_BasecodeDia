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
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdviserAvailabilitiesController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;
        private static readonly HashSet<string> AllowedDays = new(StringComparer.OrdinalIgnoreCase)
        {
            "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY"
        };

        public AdviserAvailabilitiesController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet("advisers/me/availabilities")]
        [ProducesResponseType(typeof(IEnumerable<AdviserAvailabilityResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AdviserAvailabilityResponse>>> GetMyAvailabilities()
        {
            var adviserId = await GetCurrentAdviserIdAsync();
            if (!adviserId.HasValue)
            {
                return Ok(new List<AdviserAvailabilityResponse>());
            }

            var items = await _context.AdviserAvailabilities
                .AsNoTracking()
                .Where(x => x.AdviserId == adviserId.Value && !x.IsDeleted)
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .Select(x => new AdviserAvailabilityResponse
                {
                    AvailabilityId = x.Id,
                    AdviserId = x.AdviserId,
                    DayOfWeek = x.DayOfWeek,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Location = x.Location
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("advisers/me/availability")]
        [ProducesResponseType(typeof(IEnumerable<AdviserAvailabilityResponse>), StatusCodes.Status200OK)]
        public Task<ActionResult<IEnumerable<AdviserAvailabilityResponse>>> GetMyAvailabilityAlias()
            => GetMyAvailabilities();

        [HttpGet("advisers/me/adviser-availabilities")]
        [ProducesResponseType(typeof(IEnumerable<AdviserAvailabilityResponse>), StatusCodes.Status200OK)]
        public Task<ActionResult<IEnumerable<AdviserAvailabilityResponse>>> GetMyAdviserAvailabilitiesAlias()
            => GetMyAvailabilities();

        [HttpPost("advisers/me/availabilities")]
        [ProducesResponseType(typeof(AdviserAvailabilityResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AdviserAvailabilityResponse>> CreateMyAvailability([FromBody] UpsertAdviserAvailabilityRequest request)
        {
            var adviserId = await GetCurrentAdviserIdAsync();
            if (!adviserId.HasValue)
            {
                return BadRequest(new MessageResponse { Message = "No adviser profile found for current user." });
            }

            string validationMessage;
            try
            {
                validationMessage = ValidateAvailabilityRequest(request);
            }
            catch
            {
                return BadRequest(new MessageResponse { Message = "Invalid availability payload." });
            }

            if (validationMessage != null)
            {
                return BadRequest(new MessageResponse { Message = validationMessage });
            }

            var item = new AdviserAvailability
            {
                AdviserId = adviserId.Value,
                DayOfWeek = request.DayOfWeek?.Trim().ToUpperInvariant(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Location = request.Location,
                IsDeleted = false,
                DeleteDate = null,
                DeleteName = null
            };

            _context.AdviserAvailabilities.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdviserAvailabilityById), new { availabilityId = item.Id }, new AdviserAvailabilityResponse
            {
                AvailabilityId = item.Id,
                AdviserId = item.AdviserId,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Location = item.Location
            });
        }

        [HttpPut("advisers/me/availabilities")]
        [ProducesResponseType(typeof(IEnumerable<AdviserAvailabilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AdviserAvailabilityResponse>>> SaveMyAvailabilities([FromBody] SaveMyAdviserAvailabilitiesRequest request)
        {
            var adviserId = await GetCurrentAdviserIdAsync();
            if (!adviserId.HasValue)
            {
                return BadRequest(new MessageResponse { Message = "No adviser profile found for current user." });
            }

            if (request?.Availabilities == null)
            {
                return BadRequest(new MessageResponse { Message = "Availabilities are required." });
            }

            if (request.Availabilities.Count == 0)
            {
                return BadRequest(new MessageResponse { Message = "At least one availability is required." });
            }

            for (var i = 0; i < request.Availabilities.Count; i++)
            {
                var availability = request.Availabilities[i];
                string validationMessage;
                try
                {
                    validationMessage = ValidateAvailabilityRequest(availability);
                }
                catch
                {
                    return BadRequest(new MessageResponse { Message = $"Availabilities[{i}]: Invalid availability payload." });
                }

                if (validationMessage != null)
                {
                    return BadRequest(new MessageResponse { Message = $"Availabilities[{i}]: {validationMessage}" });
                }
            }

            var now = DateTime.Now;
            var username = GetCurrentUsername();

            var existing = await _context.AdviserAvailabilities
                .Where(x => x.AdviserId == adviserId.Value && !x.IsDeleted)
                .ToListAsync();

            foreach (var item in existing)
            {
                item.IsDeleted = true;
                item.DeleteDate = now;
                item.DeleteName = username;
            }

            var newItems = request.Availabilities.Select(x => new AdviserAvailability
            {
                AdviserId = adviserId.Value,
                DayOfWeek = x.DayOfWeek?.Trim().ToUpperInvariant(),
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Location = x.Location,
                IsDeleted = false,
                DeleteDate = null,
                DeleteName = null
            }).ToList();

            _context.AdviserAvailabilities.AddRange(newItems);
            await _context.SaveChangesAsync();

            var response = newItems
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .Select(x => new AdviserAvailabilityResponse
                {
                    AvailabilityId = x.Id,
                    AdviserId = x.AdviserId,
                    DayOfWeek = x.DayOfWeek,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Location = x.Location
                })
                .ToList();

            return Ok(response);
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

        private static string ValidateAvailabilityRequest(UpsertAdviserAvailabilityRequest request)
        {
            if (request == null)
            {
                return "Availability payload is required.";
            }

            var day = request.DayOfWeek?.Trim();
            if (string.IsNullOrWhiteSpace(day))
            {
                return "DayOfWeek is required.";
            }

            if (!(AllowedDays?.Contains(day) ?? false))
            {
                return "DayOfWeek must be from Monday to Saturday.";
            }

            if (!request.StartTime.HasValue || !request.EndTime.HasValue)
            {
                return "StartTime and EndTime are required.";
            }

            if (request.EndTime <= request.StartTime)
            {
                return "EndTime must be greater than StartTime.";
            }

            return null;
        }

        private string GetCurrentUsername()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User?.Identity?.Name
                   ?? User?.FindFirst("UserName")?.Value
                   ?? "system";
        }

        private async Task<int?> GetCurrentAdviserIdAsync()
        {
            var username = GetCurrentUsername();
            if (string.IsNullOrWhiteSpace(username) || username == "system")
            {
                return null;
            }

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
            if (user == null)
            {
                return null;
            }

            var adviser = await _context.Advisers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.Id && !x.IsDeleted);

            return adviser?.Id;
        }
    }
}
