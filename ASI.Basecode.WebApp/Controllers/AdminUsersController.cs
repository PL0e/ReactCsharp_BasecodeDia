using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
    public class AdminUsersController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;

        public AdminUsersController(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.AsNoTracking()
                .OrderBy(x => x.Username)
                .Select(x => new
                {
                    userId = x.Id,
                    username = x.Username,
                    firstName = x.FirstName,
                    lastName = x.LastName,
                    role = x.Role,
                    isFirstLogin = x.IsFirstLogin,
                    isActive = x.IsActive,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(new { message = "Username and role are required." });
            }

            var role = request.Role.Trim().ToUpperInvariant();
            if (!IsAllowedRole(role))
            {
                return BadRequest(new { message = "Invalid role." });
            }

            if (await _context.Users.AnyAsync(x => x.Username == request.Username))
            {
                return Conflict(new { message = "Username already exists." });
            }

            var user = new User
            {
                Username = request.Username.Trim(),
                Password = null,
                Role = role,
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                IsFirstLogin = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await EnsureRoleProfileAsync(user, request.YearLevelId);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User pre-registered successfully.",
                userId = user.Id,
                username = user.Username,
                role = user.Role,
                isFirstLogin = user.IsFirstLogin
            });
        }

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> Delete(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deactivated successfully." });
        }

        private async Task EnsureRoleProfileAsync(User user, int? yearLevelId)
        {
            if (user.Role == "STUDENT")
            {
                var student = await _context.Students.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (student == null)
                {
                    _context.Students.Add(new Student
                    {
                        UserId = user.Id,
                        YearLevelId = yearLevelId,
                        IsDeleted = false
                    });
                }
                else
                {
                    student.YearLevelId = yearLevelId;
                    student.IsDeleted = false;
                }
            }
            else if (user.Role == "ADVISER" || user.Role == "CHAIRMAN")
            {
                var adviser = await _context.Advisers.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (adviser == null)
                {
                    _context.Advisers.Add(new Adviser
                    {
                        UserId = user.Id,
                        IsDeleted = false
                    });
                }
                else
                {
                    adviser.IsDeleted = false;
                }
            }
        }

        private static bool IsAllowedRole(string role)
        {
            return role == "ADMIN" || role == "CHAIRMAN" || role == "ADVISER" || role == "STUDENT";
        }
    }
}
