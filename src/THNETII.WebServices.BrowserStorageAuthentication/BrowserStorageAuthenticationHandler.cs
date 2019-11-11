using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THNETII.WebServices.BrowserStorageAuthentication
{
    public abstract class BrowserStorageAuthenticationHandler
        : SignInAuthenticationHandler<BrowserStorageAuthenticationOptions>
    {
        public BrowserStorageAuthenticationHandler(
            ProtectedBrowserStorage browserStorage,
            IOptionsMonitor<BrowserStorageAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            BrowserStorage = browserStorage;
        }

        protected ProtectedBrowserStorage BrowserStorage { get; }

        protected new BrowserStorageAuthenticationEvents Events
        {
            get => (BrowserStorageAuthenticationEvents)base.Events;
            set => base.Events = value;
        }

        protected override Task InitializeEventsAsync() =>
            Task.FromResult(new BrowserStorageAuthenticationEvents());

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task HandleSignInAsync(ClaimsPrincipal user,
            AuthenticationProperties properties)
        {
            properties ??= new AuthenticationProperties();
            var signInContext = new BrowserStorageSigningInContext(
                Context, Scheme, Options, user, properties
                );

            DateTimeOffset issuedUtc;
            if (signInContext.Properties.IssuedUtc.HasValue)
            {
                issuedUtc = signInContext.Properties.IssuedUtc.Value;
            }
            else
            {
                issuedUtc = Clock.UtcNow;
                signInContext.Properties.IssuedUtc = issuedUtc;
            }

            await Events.SigningIn(signInContext);

            var ticket = new AuthenticationTicket(signInContext.Principal,
                signInContext.Properties, signInContext.Scheme.Name);

            await BrowserStorage.SetAsync(GetType().FullName, "somekey", ticket);

            
        }

        protected override Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            throw new System.NotImplementedException();
        }
    }
}
