using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Authentication;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
    public static class OAuthSignOutExtensions
    {
        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, Action<OAuthSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<OAuthSignOutOptions, OAuthSignOutHandler<OAuthSignOutOptions>>(authenticationScheme, displayName: null, oauthScheme: null, configureOptions ?? throw new ArgumentNullException(nameof(configureOptions)));

        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<OAuthSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<OAuthSignOutOptions, OAuthSignOutHandler<OAuthSignOutOptions>>(authenticationScheme, displayName, oauthScheme: null, configureOptions ?? throw new ArgumentNullException(nameof(configureOptions)));

        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, string oauthScheme)
            => builder.AddOAuthSignOut<OAuthSignOutOptions, OAuthSignOutHandler<OAuthSignOutOptions>>(authenticationScheme, displayName, oauthScheme, configureOptions: null);

        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, string oauthScheme, Action<OAuthSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<OAuthSignOutOptions, OAuthSignOutHandler<OAuthSignOutOptions>>(authenticationScheme, displayName, oauthScheme, configureOptions);

        public static AuthenticationBuilder AddOAuthSignOut<TOptions, THandler>(this AuthenticationBuilder builder, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : OAuthSignOutOptions, new()
            where THandler : OAuthSignOutHandler<TOptions>
            => builder.AddOAuthSignOut<TOptions, THandler>(authenticationScheme, displayName: null, oauthScheme: null, configureOptions ?? throw new ArgumentNullException(nameof(configureOptions)));

        public static AuthenticationBuilder AddOAuthSignOut<TOptions, THandler>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<TOptions> configureOptions)
            where TOptions : OAuthSignOutOptions, new()
            where THandler : OAuthSignOutHandler<TOptions>
            => builder.AddOAuthSignOut<TOptions, THandler>(authenticationScheme, displayName, oauthScheme: null, configureOptions ?? throw new ArgumentNullException(nameof(configureOptions)));

        public static AuthenticationBuilder AddOAuthSignOut<TOptions, THandler>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, string oauthScheme)
            where TOptions : OAuthSignOutOptions, new()
            where THandler : OAuthSignOutHandler<TOptions>
            => builder.AddOAuthSignOut<TOptions, THandler>(authenticationScheme, displayName, oauthScheme, configureOptions: null);

        [SuppressMessage("Design", "CA1062: Validate arguments of public methods", Justification = "False positive", MessageId = "builder")]
        public static AuthenticationBuilder AddOAuthSignOut<TOptions, THandler>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, string oauthScheme, Action<TOptions> configureOptions)
            where TOptions : OAuthSignOutOptions, new()
            where THandler : OAuthSignOutHandler<TOptions>
        {
            Action<TOptions> preConfigureOptions;
            if (oauthScheme is null)
            {
                if (authenticationScheme is null)
                    throw new ArgumentNullException(nameof(authenticationScheme));

                preConfigureOptions = null;
            }
            else
            {
                if (authenticationScheme is null)
                    authenticationScheme = OAuthSignOutDefaults.AuthenticationScheme(oauthScheme);
                if (displayName is null)
                    displayName = OAuthSignOutDefaults.DisplayName(oauthScheme);

                preConfigureOptions = OAuthSignOutDefaults.ConfigureOptions<TOptions>(oauthScheme);
            }

            return builder.AddScheme<TOptions, THandler>(authenticationScheme, displayName, preConfigureOptions + configureOptions);
        }
    }
}
