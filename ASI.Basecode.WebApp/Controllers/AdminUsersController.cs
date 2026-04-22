using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.WebApp.Models.Admin;
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
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<AdminUserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<System.Collections.Generic.IEnumerable<AdminUserResponse>>> GetAll()
        {
            var users = await _context.Users.AsNoTracking()
                .OrderBy(x => x.Username)
                .Select(x => new AdminUserResponse
                {
                    UserId = x.Id,
                    Username = x.Username,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Role = x.Role,
                    IsFirstLogin = x.IsFirstLogin,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateAdminUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CreateAdminUserResponse>> Create([FromBody] CreateUserRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(new MessageResponse { Message = "Username and role are required." });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new MessageResponse { Message = "Email is required." });
            }

            var role = request.Role.Trim().ToUpperInvariant();
            if (!IsAllowedRole(role))
            {
                return BadRequest(new MessageResponse { Message = "Invalid role." });
            }

            if (await _context.Users.AnyAsync(x => x.Username == request.Username))
            {
                return Conflict(new MessageResponse { Message = "Username already exists." });
            }

            if (await _context.Users.AnyAsync(x => x.Email == request.Email))
            {
                return Conflict(new MessageResponse { Message = "Email already exists." });
            }

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
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

            return Ok(new CreateAdminUserResponse
            {
                Message = "User pre-registered successfully.",
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                IsFirstLogin = user.IsFirstLogin
            });
        }

        [HttpDelete("{userId:int}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MessageResponse>> Delete(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound(new MessageResponse { Message = "User not found." });
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new MessageResponse { Message = "User deactivated successfully." });
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
