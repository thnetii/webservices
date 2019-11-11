using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace THNETII.WebServices.BrowserStorageAuthentication
{
    public class BrowserStorageSigningInContext : PrincipalContext<BrowserStorageAuthenticationOptions>
    {
        public BrowserStorageSigningInContext(HttpContext context,
            AuthenticationScheme scheme,
            BrowserStorageAuthenticationOptions options,
            ClaimsPrincipal user, AuthenticationProperties properties)
            : base(context, scheme, options, properties)
        {
            Principal = user;
        }
    }
}
