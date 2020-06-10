using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace THNETII.WebServices.Authentication.Memory
{
    public class MemoryAuthenticationSignInContext : PrincipalContext<MemoryAuthenticationOptions>
    {
        public MemoryAuthenticationSignInContext(ClaimsPrincipal user,
            HttpContext context, AuthenticationScheme scheme,
            MemoryAuthenticationOptions options,
            AuthenticationProperties properties)
            : base(context, scheme, options, properties)
        {
            Principal = user;
        }
    }
}
