using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.WebApp.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly SignInManager _signInManager;

        public AuthController(AsiBasecodeDBContext context, IConfiguration configuration, SignInManager signInManager)
        {
            _context = context;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        [HttpPost("first-login/start")]
        [AllowAnonymous]
        public async Task<IActionResult> FirstLoginStart([FromBody] FirstLoginStartRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { message = "Username is required." });
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == request.Username);
            if (user == null || !user.IsActive)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                requiresPasswordSetup = user.IsFirstLogin || string.IsNullOrWhiteSpace(user.Password),
                user = new
                {
                    userId = user.Id,
                    username = user.Username,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role?.ToUpperInvariant()
                }
            });
        }

        [HttpPost("first-login/set-password")]
        [AllowAnonymous]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return BadRequest(new { message = "Password and confirm password do not match." });
            }

            if (!IsPasswordValid(request.Password))
            {
                return BadRequest(new { message = "Password must be at least 6 characters and include uppercase, letter, number, and special character." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!user.IsFirstLogin && !string.IsNullOrWhiteSpace(user.Password))
            {
                return Conflict(new { message = "Password has already been set for this account." });
            }

            user.Password = PasswordManager.EncryptPassword(request.Password);
            user.IsFirstLogin = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password set successfully." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (user.IsFirstLogin || string.IsNullOrWhiteSpace(user.Password))
            {
                return StatusCode(StatusCodes.Status428PreconditionRequired, new
                {
                    requiresPasswordSetup = true,
                    message = "First login password setup is required."
                });
            }

            var encrypted = PasswordManager.EncryptPassword(request.Password);
            var passwordMatch = string.Equals(user.Password, encrypted, StringComparison.Ordinal) ||
                                string.Equals(user.Password, request.Password, StringComparison.Ordinal);

            if (!passwordMatch)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (string.Equals(user.Password, request.Password, StringComparison.Ordinal))
            {
                user.Password = encrypted;
                await _context.SaveChangesAsync();
            }

            var identity = _signInManager.CreateClaimsIdentity(user);
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role?.ToUpperInvariant() ?? string.Empty));

            var tokenConfig = _configuration.GetTokenAuthentication();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenConfig.SecretKey));
            var tokenOptions = TokenProviderOptionsFactory.Create(tokenConfig, signingKey);
            var tokenProvider = new TokenProvider(Options.Create(tokenOptions));
            var accessToken = tokenProvider.GetJwtSecurityToken(identity, tokenOptions);

            return Ok(new
            {
                access_token = accessToken,
                expires_in = (int)tokenOptions.Expiration.TotalSeconds,
                role = user.Role?.ToUpperInvariant(),
                user = new
                {
                    userId = user.Id,
                    username = user.Username,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role?.ToUpperInvariant(),
                    isActive = user.IsActive
                }
            });
        }

        private static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            return Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*[A-Za-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{6,}$");
        }
    }
}
