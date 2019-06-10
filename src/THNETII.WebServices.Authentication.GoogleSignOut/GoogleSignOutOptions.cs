
using THNETII.WebServices.Authentication.OAuthSignOut;

namespace THNETII.WebServices.Authentication.GoogleSignOut
{
    public class GoogleSignOutOptions : OAuthSignOutOptions
    {
        public GoogleSignOutOptions()
        {
            RevokeEndpoint = GoogleSignOutDefaults.RevokeEndpoint;
        }
    }
}
