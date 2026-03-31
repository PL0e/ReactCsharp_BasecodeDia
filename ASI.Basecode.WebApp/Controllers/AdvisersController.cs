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
    public class AdvisersController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AdvisersController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var advisers = await _context.Advisers
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Join(
                    _context.Users.AsNoTracking().Where(u => u.IsActive),
                    adviser => adviser.UserId,
                    user => user.Id,
                    (adviser, user) => new
                    {
                        adviserId = adviser.Id,
                        id = adviser.Id,
                        userId = adviser.UserId,
                        username = user.Username,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        fullName = $"{user.FirstName} {user.LastName}".Trim(),
                        name = $"{user.FirstName} {user.LastName}".Trim(),
                        email = user.Username,
                        isDeleted = adviser.IsDeleted,
                        deleteDate = adviser.DeleteDate,
                        deleteName = adviser.DeleteName
                    })
                .ToListAsync();

            return Ok(advisers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var adviser = await _context.Advisers
                .AsNoTracking()
                .Where(a => a.Id == id && !a.IsDeleted)
                .Join(
                    _context.Users.AsNoTracking().Where(u => u.IsActive),
                    a => a.UserId,
                    u => u.Id,
                    (a, u) => new
                    {
                        adviserId = a.Id,
                        id = a.Id,
                        userId = a.UserId,
                        username = u.Username,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        fullName = $"{u.FirstName} {u.LastName}".Trim(),
                        name = $"{u.FirstName} {u.LastName}".Trim(),
                        email = u.Username,
                        isDeleted = a.IsDeleted,
                        deleteDate = a.DeleteDate,
                        deleteName = a.DeleteName
                    })
                .FirstOrDefaultAsync();

            if (adviser == null) return NotFound();
            return Ok(adviser);
        }

        [HttpPost]
        public async Task<ActionResult<Adviser>> Create([FromBody] Adviser adviser)
        {
            adviser.IsDeleted = false;
            adviser.DeleteDate = null;
            adviser.DeleteName = null;
            _context.Advisers.Add(adviser);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = adviser.Id }, adviser);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Adviser adviser)
        {
            if (id != adviser.Id) return BadRequest();
            var existing = await _context.Advisers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existing == null) return NotFound();

            existing.UserId = adviser.UserId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var adviser = await _context.Advisers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (adviser == null) return NotFound();

            adviser.IsDeleted = true;
            adviser.DeleteDate = DateTime.Now;
            adviser.DeleteName = User?.Identity?.Name ?? User?.FindFirst("UserName")?.Value ?? "system";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("directory")]
        public async Task<IActionResult> GetAdviserDirectory()
        {
            var advisers = await _context.Advisers
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Join(
                    _context.Users.AsNoTracking().Where(u => u.IsActive && u.Role == "ADVISER"),
                    adviser => adviser.UserId,
                    user => user.Id,
                    (adviser, user) => new
                    {
                        adviserId = adviser.Id,
                        userId = user.Id,
                        username = user.Username,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        role = user.Role
                    })
                .ToListAsync();

            return Ok(advisers);
        }
    }
}
