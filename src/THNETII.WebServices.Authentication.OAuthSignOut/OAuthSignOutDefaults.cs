using System;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
    public static class OAuthSignOutDefaults
    {
        public static string RevokeEndpointFallback(OAuthOptions options)
        {
            if (options is null)
                return null;

            try
            {
                var authorizeUri = new Uri(options.AuthorizationEndpoint);
                return new Uri(authorizeUri, "revoke").ToString();
            }
            catch (UriFormatException) { return null; }
        }

        public static readonly string AuthenticationSchemeSuffix = "-SignOut";

        public static readonly string DisplayNameSuffix = " (Sign-Out)";

        public static string AuthenticationScheme(string oauthScheme) =>
            oauthScheme + AuthenticationSchemeSuffix;

        public static string DisplayName(string oauthDisplayName) =>
            oauthDisplayName + DisplayNameSuffix;

        public static Action<TOptions> ConfigureOptions<TOptions>(string oauthScheme)
            where TOptions : OAuthSignOutOptions
            => options => options.RemoteAuthenticationScheme = oauthScheme;
    }
}
