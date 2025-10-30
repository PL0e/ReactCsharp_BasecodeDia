using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.WebApp.Extensions.Configuration;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Authentication
{
    public class SignInManager
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginUser user { get; set; }

        public SignInManager() { }

        public SignInManager(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._configuration = configuration;
            this._httpContextAccessor = httpContextAccessor;
            user = new LoginUser();
        }

        public Task<ClaimsIdentity> GetClaimsIdentity(string username, string password)
        {
            ClaimsIdentity claimsIdentity = null;
            User userData = new User();

            user.loginResult = LoginResult.Success; // TODO: replace with actual authentication logic

            if (user.loginResult == LoginResult.Failed)
            {
                return Task.FromResult<ClaimsIdentity>(null);
            }

            user.userData = userData;
            claimsIdentity = CreateClaimsIdentity(userData);
            return Task.FromResult(claimsIdentity);
        }

        public ClaimsIdentity CreateClaimsIdentity(User user)
        {
            var token = _configuration.GetTokenAuthentication();
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String, Const.Issuer),
                new Claim(ClaimTypes.Name, user.Name, ClaimValueTypes.String, Const.Issuer),

                new Claim("UserId", user.Id.ToString(), ClaimValueTypes.String, Const.Issuer),
                new Claim("UserName", user.Name, ClaimValueTypes.String, Const.Issuer),
            };
            return new ClaimsIdentity(claims, Const.AuthenticationScheme);
        }

        public IPrincipal CreateClaimsPrincipal(ClaimsIdentity identity)
        {
            var identities = new List<ClaimsIdentity> { identity };
            return CreateClaimsPrincipal(identities);
        }

        public IPrincipal CreateClaimsPrincipal(IEnumerable<ClaimsIdentity> identities)
        {
            return new ClaimsPrincipal(identities);
        }

        public async Task SignInAsync(User user, bool isPersistent = false)
        {
            var claimsIdentity = CreateClaimsIdentity(user);
            var principal = CreateClaimsPrincipal(claimsIdentity);
            await SignInAsync(principal, isPersistent);
        }

        public async Task SignInAsync(IPrincipal principal, bool isPersistent = false)
        {
            var token = _configuration.GetTokenAuthentication();
            await _httpContextAccessor.HttpContext.SignInAsync(
                Const.AuthenticationScheme,
                (ClaimsPrincipal)principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(token.ExpirationMinutes),
                    IsPersistent = isPersistent,
                    AllowRefresh = false
                });
        }

        public async Task SignOutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(Const.AuthenticationScheme);
        }
    }
}
