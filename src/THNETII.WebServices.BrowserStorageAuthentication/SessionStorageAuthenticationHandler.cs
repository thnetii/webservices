using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THNETII.WebServices.BrowserStorageAuthentication
{
    public class SessionStorageAuthenticationHandler
        : BrowserStorageAuthenticationHandler
    {
        public SessionStorageAuthenticationHandler(
            ProtectedSessionStorage sessionStorage,
            IOptionsMonitor<BrowserStorageAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(sessionStorage, options, logger, encoder, clock) { }

        protected ProtectedSessionStorage SessionStorage =>
            (ProtectedSessionStorage)BrowserStorage;
    }
}
