using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace THNETII.WebServices.BrowserStorageAuthentication
{
    public class BrowserStorageAuthenticationOptions : AuthenticationSchemeOptions
    {
        public BrowserStorageAuthenticationOptions() : base()
        {
            Events = new BrowserStorageAuthenticationEvents();
        }

        public new BrowserStorageAuthenticationEvents Events
        {
            get => (BrowserStorageAuthenticationEvents)base.Events;
            set => base.Events = value;
        }

        public IDataProtectionProvider DataProtectionProvider { get; set; }

        public ISecureDataFormat<AuthenticationTicket> TicketDataFormat { get; set; }
    }
}
