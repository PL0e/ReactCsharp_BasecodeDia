using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;

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

        /// <summary>
        /// Authenticates a user and returns a JWT access token.
        /// POST /api/Account/Login
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(model.UserId)
                || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(ApiResult<object>.CreateError("UserId and Password are required."));
            }

            User user = null;
            var loginResult = _userService.AuthenticateUser(model.UserId, model.Password, ref user);

            if (loginResult == LoginResult.Failed)
                return Unauthorized(ApiResult<object>.CreateError("Invalid ID or password."));

            // Role comes directly from the database record
            string role = user.Role;

            if (string.IsNullOrWhiteSpace(role))
            {
                return Unauthorized(ApiResult<object>.CreateError("User account has no assigned role."));
            }

            var identity = _signInManager.CreateClaimsIdentity(user);
            identity.AddClaim(new Claim(ClaimTypes.Role, role));

            var tokenConfig = _appConfiguration.GetTokenAuthentication();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenConfig.SecretKey));
            var tokenOptions = TokenProviderOptionsFactory.Create(tokenConfig, signingKey);
            var tokenProvider = new TokenProvider(Options.Create(tokenOptions));
            var accessToken = tokenProvider.GetJwtSecurityToken(identity, tokenOptions);

            // Redact password hash before sending to client
            user.Password = null;

            var response = new LoginUser
            {
                loginResult = loginResult,
                access_token = accessToken,
                expires_in = (int)tokenOptions.Expiration.TotalSeconds,
                userData = user,
                message = "Login successful."
            };

            return Ok(ApiResult<object>.CreateSuccess(response, "Login successful."));
        }

        /// <summary>
        /// Registers a user account and persists it in the database.
        /// POST /api/Account/Register
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (model == null)
            {
                return BadRequest(ApiResult<object>.CreateError("Request body is required."));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResult<object>.CreateError("Invalid registration data."));
            }

            if (!Regex.IsMatch(model.UserId, "^(10|20|30)\\d{8}$"))
            {
                return BadRequest(ApiResult<object>.CreateError("UserId must be 10 digits and start with 10, 20, or 30."));
            }

            if (!Regex.IsMatch(model.Password, "^(?=.*[A-Za-z])(?=.*\\d).{8,}$"))
            {
                return BadRequest(ApiResult<object>.CreateError("Password must be at least 8 characters and include both letters and numbers."));
            }

            if (_userService.UserExists(model.UserId))
            {
                return Conflict(ApiResult<object>.CreateError("User ID already exists."));
            }

            try
            {
                _userService.RegisterUser(model.UserId, model.Name.Trim(), model.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {UserId}", model.UserId);
                return StatusCode(500, ApiResult<object>.CreateError("Registration failed due to a server error. Please try again."));
            }

            return Ok(ApiResult<object>.CreateSuccess(new
            {
                userId = model.UserId,
                name = model.Name.Trim()
            }, "Account created successfully."));
        }

        /// <summary>
        /// Signs out the current user.
        /// POST /api/Account/Logout
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(ApiResult<object>.CreateSuccess("Signed out successfully."));
        }
    }
}
