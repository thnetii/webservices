using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THNETII.WebServices.BrowserStorageAuthentication
{
    public class LocalStorageAuthenticationHandler : BrowserStorageAuthenticationHandler
    {
        public LocalStorageAuthenticationHandler(
            ProtectedLocalStorage localStorage,
            IOptionsMonitor<BrowserStorageAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(localStorage, options, logger, encoder, clock) { }

        protected ProtectedLocalStorage LocalStorage =>
            (ProtectedLocalStorage)BrowserStorage;
    }
}
