using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = _userService.GetEmail(model.Email);
            if (existing != null)
                return BadRequest(new { message = "Email already exists!" });

            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await _userService.AddAsync(model);
            return Ok(new { message = "User registered successfully!" });
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok("AccountController is working!");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetUsers()
        {
            var users = _userService.GetUsers();
            return Ok(users);
        }


        /// <summary>
        /// Login Method
        /// </summary>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userService.GetEmail(model.Email); 
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (!isPasswordValid)
                return Unauthorized(new { message = "Invalid email or password." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) 
            };

            var identity = new ClaimsIdentity(claims, Const.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);


      
            await HttpContext.SignInAsync(Const.AuthenticationScheme, principal);

            return Ok(new
            {
                message = "Login successful!",
                user = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.Role
                }
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // ✅ Return only safe fields
            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User model)
        {
            try
            {
                var existingUser = _userService.GetById(id);
                if (existingUser == null)
                    return NotFound(new { message = "User not found." });

                // Update fields
                existingUser.Name = model.Name;
                existingUser.Email = model.Email;
                existingUser.Role = model.Role;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                await _userService.UpdateAsync(existingUser);

                return Ok(new { message = "User updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating user.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            await _userService.DeleteAsync(user);
            return Ok(new { message = "User deleted successfully!" });
        }


        /// <summary>
        /// Sign Out current account
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return Ok();
        }
    }
}
