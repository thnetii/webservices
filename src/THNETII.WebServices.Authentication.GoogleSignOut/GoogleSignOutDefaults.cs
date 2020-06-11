using Microsoft.AspNetCore.Authentication.Google;

using THNETII.WebServices.Authentication.OAuthSignOut;

namespace THNETII.WebServices.Authentication.GoogleSignOut
{
    public static class GoogleSignOutDefaults
    {
        public static readonly string AuthenticationScheme = OAuthSignOutDefaults.AuthenticationScheme(GoogleDefaults.AuthenticationScheme);

        public static readonly string DisplayName = OAuthSignOutDefaults.DisplayName(GoogleDefaults.DisplayName);

        public static readonly string RevokeEndpoint = "https://accounts.google.com/o/oauth2/revoke";
    }
}
