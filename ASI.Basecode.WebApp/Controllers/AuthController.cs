using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.WebApp.Models.Api;
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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var loginIdentifier = request.Username.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.IsActive &&
                (x.Username == loginIdentifier || x.Email == loginIdentifier));
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (user.IsFirstLogin || string.IsNullOrWhiteSpace(user.Password))
            {
                return StatusCode(StatusCodes.Status428PreconditionRequired, new
                {
                    requiresPasswordSetup = true,
                    message = "Please use 'Forgot Password' to set your initial password."
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

        [HttpPost("forgot-password/start")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ForgotPasswordStartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ForgotPasswordStartResponse>> ForgotPasswordStart([FromBody] ForgotPasswordStartRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new MessageResponse { Message = "Email is required." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive);
            if (user == null)
            {
                return NotFound(new MessageResponse { Message = "Email not found or user is not active." });
            }

            var response = new ForgotPasswordStartResponse
            {
                Message = "Email verified. You can proceed to reset your password.",
                Username = user.Username
            };

            return Ok(response);
        }

        [HttpPost("forgot-password/reset")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MessageResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new MessageResponse { Message = "Email and new password are required." });
            }

            var identifier = request.Email.Trim();

            if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return BadRequest(new MessageResponse { Message = "Password and confirm password do not match." });
            }

            if (!IsPasswordValid(request.NewPassword))
            {
                return BadRequest(new MessageResponse { Message = "Password must be at least 6 characters and include uppercase, letter, number, and special character." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.IsActive &&
                (x.Email == identifier || x.Username == identifier));
            if (user == null)
            {
                return BadRequest(new MessageResponse { Message = "Email not found or user is not active." });
            }

            user.Password = PasswordManager.EncryptPassword(request.NewPassword);
            user.IsFirstLogin = false;
            await _context.SaveChangesAsync();

            return Ok(new MessageResponse { Message = "Password reset successfully." });
        }

        private static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            return Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*[A-Za-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{6,}$");
        }
    }
}
