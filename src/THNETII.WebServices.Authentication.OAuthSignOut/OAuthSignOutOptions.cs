using System;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
    public class OAuthSignOutOptions : AuthenticationSchemeOptions
    {
        public OAuthSignOutOptions() : base()
        {
            Events = new OAuthSignOutEvents();
        }

        public new OAuthSignOutEvents Events
        {
            get => (OAuthSignOutEvents)base.Events;
            set => base.Events = value;
        }

        public string RemoteAuthenticationScheme { get; set; }

        public string RevokeEndpoint { get; set; }

        public virtual Func<OAuthOptions, string> RevokeEndpointFallback { get; set; } =
            OAuthSignOutDefaults.RevokeEndpointFallback;

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(RemoteAuthenticationScheme))
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new ArgumentException($"The '{nameof(RemoteAuthenticationScheme)}' option must be provided.", nameof(RemoteAuthenticationScheme));
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }
        }
    }
}
