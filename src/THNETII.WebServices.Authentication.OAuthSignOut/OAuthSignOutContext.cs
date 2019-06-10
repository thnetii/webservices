
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
    public class OAuthSignOutContext : PrincipalContext<OAuthSignOutOptions>
    {
        public OAuthSignOutContext(HttpContext context,
            AuthenticationScheme scheme, OAuthSignOutOptions options,
            AuthenticationProperties properties,
            IAuthenticationRequestHandler oauthHandler,
            AuthenticationTicket authTicket)
            : base(context, scheme, options, properties)
        {
            Principal = authTicket?.Principal ?? context?.User;
            Ticket = authTicket;

            OAuthHandler = oauthHandler;
        }

        public AuthenticationTicket Ticket { get; set; }
        public IAuthenticationRequestHandler OAuthHandler { get; set; }
    }
}
