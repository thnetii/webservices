using System;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

using THNETII.WebServices.Authentication.OAuthSignOut;

namespace THNETII.WebServices.Authentication.GoogleSignOut
{
    public static class GoogleSignOutExtensions
    {
        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, Action<GoogleSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<GoogleSignOutOptions, OAuthSignOutHandler<GoogleSignOutOptions>>(authenticationScheme, GoogleSignOutDefaults.DisplayName, GoogleDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<GoogleSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<GoogleSignOutOptions, OAuthSignOutHandler<GoogleSignOutOptions>>(authenticationScheme, displayName, GoogleDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, string oauthScheme)
            => builder.AddOAuthSignOut<GoogleSignOutOptions, OAuthSignOutHandler<GoogleSignOutOptions>>(authenticationScheme, displayName, oauthScheme, configureOptions: null);

        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, string oauthScheme, Action<GoogleSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<GoogleSignOutOptions, OAuthSignOutHandler<GoogleSignOutOptions>>(authenticationScheme, displayName, oauthScheme, configureOptions);
    }
}
