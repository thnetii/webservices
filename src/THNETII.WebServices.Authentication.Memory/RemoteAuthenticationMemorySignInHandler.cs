using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace THNETII.WebServices.Authentication.Memory
{
    public class RemoteAuthenticationMemorySignInHandler
        : SignInAuthenticationHandler<AuthenticationSchemeOptions>
    {
        public RemoteAuthenticationMemorySignInHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            

            throw new NotImplementedException();
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            

            throw new NotImplementedException();
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }
    }
}
