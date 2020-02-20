using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Security
{
    public class AuthenticationHelper
    {
        private HttpContext _httpContext;
        private QPublishingOptions _options;
        public AuthenticationHelper (IHttpContextAccessor httpContextAccessor, QPublishingOptions options)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _options = options;
        }

        public async void SignIn(QpUser user)
        {
            var principal = GetClaimsPrincipal(user);
            var authProperties = new AuthenticationProperties();
            await _httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);
        }

        public static ClaimsPrincipal GetClaimsPrincipal(QpUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, string.Join(";", user.Roles)),
                new Claim("CustomerCode", user.CustomerCode ?? ""),
                new Claim("LanguageId", user.LanguageId.ToString()),
                new Claim("CultureName", user.CultureName),
                new Claim("MustChangePassword", user.MustChangePassword.ToString()),
                new Claim(ClaimTypes.Sid, user.SessionId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(claimsIdentity);

        }

        public async void SignOut()
        {
            await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

    }
}
