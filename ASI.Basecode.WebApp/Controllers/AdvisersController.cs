using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdvisersController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AdvisersController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<AdviserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<System.Collections.Generic.IEnumerable<AdviserResponse>>> GetAll()
        {
            var advisers = await _context.Advisers
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Join(
                    _context.Users.AsNoTracking().Where(u => u.IsActive),
                    adviser => adviser.UserId,
                    user => user.Id,
                    (adviser, user) => new AdviserResponse
                    {
                        AdviserId = adviser.Id,
                        UserId = adviser.UserId,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = $"{user.FirstName} {user.LastName}".Trim(),
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Email = user.Username,
                        IsDeleted = adviser.IsDeleted,
                        DeleteDate = adviser.DeleteDate,
                        DeleteName = adviser.DeleteName
                    })
                .ToListAsync();

            return Ok(advisers);
        }

        [HttpGet("{adviserId:int}")]
        [ProducesResponseType(typeof(AdviserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdviserResponse>> GetById(int adviserId)
        {
            var adviser = await _context.Advisers
                .AsNoTracking()
                .Where(a => a.Id == adviserId && !a.IsDeleted)
                .Join(
                    _context.Users.AsNoTracking().Where(u => u.IsActive),
                    a => a.UserId,
                    u => u.Id,
                    (a, u) => new AdviserResponse
                    {
                        AdviserId = a.Id,
                        UserId = a.UserId,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}".Trim(),
                        Name = $"{u.FirstName} {u.LastName}".Trim(),
                        Email = u.Username,
                        IsDeleted = a.IsDeleted,
                        DeleteDate = a.DeleteDate,
                        DeleteName = a.DeleteName
                    })
                .FirstOrDefaultAsync();

            if (adviser == null) return NotFound();
            return Ok(adviser);
        }

        [HttpPost]
        public async Task<ActionResult<AdviserResponse>> Create([FromBody] UpsertAdviserRequest request)
        {
            var adviser = new Adviser
            {
                UserId = request.UserId,
                IsDeleted = false,
                DeleteDate = null,
                DeleteName = null
            };

            _context.Advisers.Add(adviser);
            await _context.SaveChangesAsync();

            var response = await _context.Advisers.AsNoTracking()
                .Where(a => a.Id == adviser.Id)
                .Join(
                    _context.Users.AsNoTracking(),
                    a => a.UserId,
                    u => u.Id,
                    (a, u) => new AdviserResponse
                    {
                        AdviserId = a.Id,
                        UserId = a.UserId,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FullName = $"{u.FirstName} {u.LastName}".Trim(),
                        Name = $"{u.FirstName} {u.LastName}".Trim(),
                        Email = u.Username,
                        IsDeleted = a.IsDeleted,
                        DeleteDate = a.DeleteDate,
                        DeleteName = a.DeleteName
                    })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetById), new { adviserId = adviser.Id }, response);
        }

        [HttpPut("{adviserId:int}")]
        public async Task<IActionResult> Update(int adviserId, [FromBody] UpsertAdviserRequest request)
        {
            var existing = await _context.Advisers.FirstOrDefaultAsync(x => x.Id == adviserId && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.UserId = request.UserId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{adviserId:int}")]
        public async Task<IActionResult> Delete(int adviserId)
        {
            var adviser = await _context.Advisers.FirstOrDefaultAsync(x => x.Id == adviserId && !x.IsDeleted);
            if (adviser == null) return NotFound();

            adviser.IsDeleted = true;
            adviser.DeleteDate = DateTime.Now;
            adviser.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("directory")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<AdviserDirectoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<System.Collections.Generic.IEnumerable<AdviserDirectoryResponse>>> GetAdviserDirectory()
        {
            var advisers = await _context.Advisers
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Join(
                    _context.Users.AsNoTracking().Where(u => u.IsActive && u.Role == "ADVISER"),
                    adviser => adviser.UserId,
                    user => user.Id,
                    (adviser, user) => new AdviserDirectoryResponse
                    {
                        AdviserId = adviser.Id,
                        UserId = user.Id,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role
                    })
                .ToListAsync();

            return Ok(advisers);
        }
    }
}
