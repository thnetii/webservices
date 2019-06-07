using System;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using THNETII.WebServices.Authentication.OAuthSignOut;

namespace THNETII.WebServices.Authentication.GoogleSignOut
{
    public static class GoogleSignOutExtensions
    {
        public static AuthenticationBuilder AddGoogleSignOut(this AuthenticationBuilder builder)
            => builder.AddGoogleSignOut(GoogleSignOutDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddGoogleSignOut(this AuthenticationBuilder builder, Action<GoogleSignOutOptions> configureOptions)
            => builder.AddGoogleSignOut(GoogleSignOutDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddGoogleSignOut(this AuthenticationBuilder builder, string authenticationScheme, Action<GoogleSignOutOptions> configureOptions)
            => builder.AddGoogleSignOut(authenticationScheme, GoogleSignOutDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddGoogleSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<GoogleSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<GoogleSignOutOptions, GoogleSignOutHandler>(authenticationScheme, displayName, configureOptions);
    }

    public static class GoogleSignOutDefaults
    {
        public static readonly string AuthenticationScheme = OAuthSignOutDefaults.AuthenticationScheme(GoogleDefaults.AuthenticationScheme);

        public static readonly string DisplayName = OAuthSignOutDefaults.DisplayName(GoogleDefaults.DisplayName);

        public static readonly string RevokeEndpoint = "https://accounts.google.com/o/oauth2/revoke";
    }

    public class GoogleSignOutHandler : OAuthSignOutHandler<GoogleSignOutOptions>
    {
        public GoogleSignOutHandler(IAuthenticationSchemeProvider schemeProvider,
            IAuthenticationHandlerProvider handlerProvider,
            IOptionsMonitor<GoogleSignOutOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock)
            : base(schemeProvider, handlerProvider, options, logger, encoder, clock) { }

        //protected override Task RevokeTokenAsync<TOAuthOptions>(
        //    OAuthHandler<TOAuthOptions> oauthHandler,
        //    AuthenticationToken token)
        //{
        //    // Refresh tokens are automatically revoked with the corresponding
        //    // access token
        //    if (token.Name != "access_token")
        //        return Task.CompletedTask;

        //    return base.RevokeTokenAsync(oauthHandler, token);
        //}
    }

    public class GoogleSignOutOptions : OAuthSignOutOptions
    {
        public GoogleSignOutOptions()
        {
            RevokeEndpoint = GoogleSignOutDefaults.RevokeEndpoint;
        }
    }
}
