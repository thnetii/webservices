using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace THNETII.WebServices.Authentication.Memory
{
    public class MemoryAuthenticationHandler
        : SignInAuthenticationHandler<MemoryAuthenticationOptions>
    {
        private readonly MemoryAuthenticationTicketStore ticketStore;

        public MemoryAuthenticationHandler(
            MemoryAuthenticationTicketStore ticketStore,
            IOptionsMonitor<MemoryAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            this.ticketStore = ticketStore;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            

            throw new NotImplementedException();
        }

        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            var ticket = new AuthenticationTicket(user, properties, Scheme.Name);
            var context = new MemoryAuthenticationSignInContext(user,
                Context, Scheme, Options, properties);
            throw new NotImplementedException();
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }
    }
}
