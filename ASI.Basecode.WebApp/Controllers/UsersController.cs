using ASI.Basecode.Data;
using ASI.Basecode.WebApp.Models.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public UsersController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserSummaryResponse>>> GetAll()
        {
            var users = await _context.Users.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Select(x => new UserSummaryResponse
                {
                    UserId = x.Id,
                    StudentId = x.Role == "STUDENT" ? x.Username : null,
                    Username = x.Username,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Role = x.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{userId:int}")]
        [ProducesResponseType(typeof(UserSummaryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserSummaryResponse>> GetById(int userId)
        {
            var user = await _context.Users.AsNoTracking()
                .Where(x => x.Id == userId && x.IsActive)
                .Select(x => new UserSummaryResponse
                {
                    UserId = x.Id,
                    StudentId = x.Role == "STUDENT" ? x.Username : null,
                    Username = x.Username,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Role = x.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("role/{role}")]
        [ProducesResponseType(typeof(IEnumerable<UserSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<UserSummaryResponse>>> GetByRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new MessageResponse { Message = "Role is required." });
            }

            var normalizedRole = role.Trim().ToUpperInvariant();

            var users = await _context.Users.AsNoTracking()
                .Where(x => x.IsActive && x.Role == normalizedRole)
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Select(x => new UserSummaryResponse
                {
                    UserId = x.Id,
                    StudentId = x.Role == "STUDENT" ? x.Username : null,
                    Username = x.Username,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Role = x.Role
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
