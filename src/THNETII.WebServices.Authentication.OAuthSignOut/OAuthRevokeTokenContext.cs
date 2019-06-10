
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
    public class OAuthRevokeTokenContext : PropertiesContext<OAuthSignOutOptions>
    {
        public OAuthRevokeTokenContext(HttpContext context,
            AuthenticationScheme scheme, OAuthSignOutOptions options,
            AuthenticationProperties properties, AuthenticationToken token,
            IAuthenticationRequestHandler oauthHandler)
            : base(context, scheme, options, properties)
        {
            Token = token;
            OAuthHandler = oauthHandler;
        }

        public AuthenticationToken Token { get; set; }

        public IAuthenticationRequestHandler OAuthHandler { get; set; }
    }
}
