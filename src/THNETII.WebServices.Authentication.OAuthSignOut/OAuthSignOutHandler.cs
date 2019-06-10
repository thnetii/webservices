using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
    public class OAuthSignOutHandler<TOptions> : SignOutAuthenticationHandler<TOptions>
        where TOptions : OAuthSignOutOptions, new()
    {
        protected new OAuthSignOutEvents Events
        {
            get => (OAuthSignOutEvents)base.Events;
            set => base.Events = value;
        }
        protected virtual IAuthenticationSchemeProvider SchemeProvider { get; }
        protected virtual IAuthenticationHandlerProvider HandlerProvider { get; }

        public OAuthSignOutHandler(
            IAuthenticationSchemeProvider schemeProvider,
            IAuthenticationHandlerProvider handlerProvider,
            IOptionsMonitor<TOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            SchemeProvider = schemeProvider;
            HandlerProvider = handlerProvider;
        }

        protected override Task<object> CreateEventsAsync() =>
            Task.FromResult<object>(new OAuthSignOutEvents());

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
            Task.FromResult(AuthenticateResult.NoResult());

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected sealed override async Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            var oauthScheme = await SchemeProvider.GetSchemeAsync(Options.RemoteAuthenticationScheme);
            var remoteHandler = await HandlerProvider.GetHandlerAsync(Context, oauthScheme?.Name);
            if (remoteHandler is IAuthenticationSignOutHandler signOutHandler)
            {
                await signOutHandler.SignOutAsync(properties);
                return;
            }

            var oauthOptionType = OAuthOptionsHelper.GetOAuthOptionsType(remoteHandler?.GetType());
            if (oauthOptionType is null)
            {
                throw new InvalidOperationException($"Handler of type {oauthScheme.HandlerType} does not implement {typeof(OAuthHandler<>)}. Options type deriving from {typeof(OAuthOptions)} could not be determined.");
            }

            await handleSignOutFuncs
                .GetOrAdd(oauthOptionType, CreateHandleSignOutFunc)
                .Invoke(this, remoteHandler, properties);
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected virtual async Task HandleSignOutAsync<TOAuthOptions>(
            OAuthHandler<TOAuthOptions> oauthHandler,
            AuthenticationProperties properties)
            where TOAuthOptions : OAuthOptions, new()
        {
            var signoutContext = new OAuthSignOutContext(Context, Scheme, Options, properties, oauthHandler);

            await Events.SigningOut(signoutContext);

            string oauthSchemeName = oauthHandler?.Scheme.Name;

            foreach (var token in properties.GetTokens())
            {
                switch (token.Name)
                {
                    case "access_token":
                    case "refresh_token":
                        var revokeContext = new OAuthRevokeTokenContext(
                            Context, Scheme, Options, properties, token,
                            oauthHandler);

                        await Events.RevokeToken(revokeContext);
                        try
                        {
                            await RevokeTokenAsync(oauthHandler, token);
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch (Exception revokeExcept)
                        {
                            Logger.TokenRevokeFailure(token.Name, oauthSchemeName, revokeExcept);
                        }
#pragma warning restore CA1031 // Do not catch general exception types
                        break;
                }
            }

            Logger.SignedOut(oauthSchemeName);

            string redirectUri = properties?.RedirectUri;
            if (!string.IsNullOrEmpty(redirectUri))
                Context.Response.Redirect(redirectUri);
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected virtual async Task RevokeTokenAsync<TOAuthOptions>(
            OAuthHandler<TOAuthOptions> oauthHandler, AuthenticationToken token)
            where TOAuthOptions : OAuthOptions, new()
        {
            var options = oauthHandler?.Options;
            var revokeEndpoint = GetRevokeEndpoint(options);
            if (revokeEndpoint is null)
                return;
            var tokenDict = token is null
                ? Enumerable.Empty<KeyValuePair<string, string>>()
                : new Dictionary<string, string>
                {
                    ["token"] = token.Value,
                    ["token_type_hint"] = token.Name
                };
            var requestContent = new FormUrlEncodedContent(tokenDict);
            using (requestContent)
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, revokeEndpoint)
            {
                Content = requestContent
            })
            using (var response = await options.Backchannel.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        protected virtual string GetRevokeEndpoint(OAuthOptions oAuthOptions) =>
            Options.RevokeEndpoint ?? Options.RevokeEndpointFallback?.Invoke(oAuthOptions);

        private delegate Task HandleSignOutFunc(
            OAuthSignOutHandler<TOptions> signOutHandler,
            IAuthenticationHandler oauthHandler,
            AuthenticationProperties properties);

        private static readonly ConcurrentDictionary<Type, HandleSignOutFunc> handleSignOutFuncs =
            new ConcurrentDictionary<Type, HandleSignOutFunc>();

        private static readonly MethodInfo HandleSignOutImplGeneric =
            ((HandleSignOutFunc)HandleSignOutImpl<OAuthOptions>)
            .Method.GetGenericMethodDefinition();

        private static HandleSignOutFunc CreateHandleSignOutFunc(Type oauthOptionsType)
        {
            return (HandleSignOutFunc)HandleSignOutImplGeneric
                .MakeGenericMethod(oauthOptionsType)
                .CreateDelegate(typeof(HandleSignOutFunc));
        }

        private static Task HandleSignOutImpl<TOAuthOptions>(
            OAuthSignOutHandler<TOptions> signOutHandler,
            IAuthenticationHandler oauthHandler,
            AuthenticationProperties properties)
            where TOAuthOptions : OAuthOptions, new()
        {
            return signOutHandler.HandleSignOutAsync(
                (OAuthHandler<TOAuthOptions>)oauthHandler,
                properties);
        }
    }

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
    }

    internal static class OAuthSignOutLoggerExtensions
    {
        private static readonly Action<ILogger, string, string, Exception> tokenRevoked =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(21, "OAuthTokenRevoked"),
                "OAuthToken '{TokenName}' was revoked for authentication scheme '{OAuthScheme}'");

        private static readonly Action<ILogger, string, string, Exception> tokenRevokeFailure =
            LoggerMessage.Define<string, string>(
                LogLevel.Warning, new EventId(22, "OAuthTokenRevokeFailure"),
                "OAuthToken '{TokenName}' revocation for authentication scheme '{OAuthScheme}' failed.");

        private static readonly Action<ILogger, string, Exception> signedOut =
            LoggerMessage.Define<string>(
                LogLevel.Debug, new EventId(20, "OAuthSignedOut"),
                "AuthenticationScheme: {AuthenticationScheme} was signed out.");

        public static void TokenRevoked(this ILogger logger, string tokenName, string oauthScheme) =>
            tokenRevoked(logger, tokenName, oauthScheme, null);

        public static void TokenRevokeFailure(this ILogger logger, string tokenName, string oauthScheme, Exception exception) =>
            tokenRevokeFailure(logger, tokenName, oauthScheme, exception);

        public static void SignedOut(this ILogger logger, string oauthScheme) =>
            signedOut(logger, oauthScheme, null);
    }

    public class OAuthSignOutEvents
    {
        public Func<OAuthRevokeTokenContext, Task> OnRevokeToken { get; set; } = _ => Task.CompletedTask;

        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        public Func<OAuthSignOutContext, Task> OnSigningOut { get; set; } = context => Task.CompletedTask;

        public virtual Task RevokeToken(OAuthRevokeTokenContext context) => OnRevokeToken(context);

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context"></param>
        public virtual Task SigningOut(OAuthSignOutContext context) => OnSigningOut(context);
    }

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

    public class OAuthSignOutContext : PropertiesContext<OAuthSignOutOptions>
    {
        public OAuthSignOutContext(HttpContext context,
            AuthenticationScheme scheme, OAuthSignOutOptions options,
            AuthenticationProperties properties,
            IAuthenticationRequestHandler oauthHandler)
            : base(context, scheme, options, properties)
        {
            OAuthHandler = oauthHandler;
        }

        public IAuthenticationRequestHandler OAuthHandler { get; set; }
    }

    public static class OAuthSignOutExtensions
    {
        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, Action<OAuthSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<OAuthSignOutOptions, OAuthSignOutHandler<OAuthSignOutOptions>>(authenticationScheme, configureOptions);

        public static AuthenticationBuilder AddOAuthSignOut(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<OAuthSignOutOptions> configureOptions)
            => builder.AddOAuthSignOut<OAuthSignOutOptions, OAuthSignOutHandler<OAuthSignOutOptions>>(authenticationScheme, displayName, configureOptions);

        public static AuthenticationBuilder AddOAuthSignOut<TOptions, THandler>(this AuthenticationBuilder builder, string authenticationScheme, Action<TOptions> configureOptions)
            where TOptions : OAuthSignOutOptions, new()
            where THandler : OAuthSignOutHandler<TOptions>
            => builder.AddOAuthSignOut<TOptions, THandler>(authenticationScheme, OAuthSignOutDefaults.DisplayName(OAuthDefaults.DisplayName), configureOptions);

        [SuppressMessage("Design", "CA1062: Validate arguments of public methods", Justification = "Extension Method", MessageId = "builder")]
        public static AuthenticationBuilder AddOAuthSignOut<TOptions, THandler>(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<TOptions> configureOptions)
            where TOptions : OAuthSignOutOptions, new()
            where THandler : OAuthSignOutHandler<TOptions>
            => builder.AddScheme<TOptions, THandler>(authenticationScheme, displayName, configureOptions);
    }

    internal static class OAuthOptionsHelper
    {
        public static Type GetOAuthOptionsType(Type handlerType)
        {
            var oauthHandlerType = WalkTypeHierarchy(handlerType)
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(OAuthHandler<>));
            if (oauthHandlerType is null)
                return null;

            return oauthHandlerType.GetGenericArguments()[0];
        }

        private static IEnumerable<Type> WalkTypeHierarchy(Type type)
        {
            for (var t = type; type is Type; t = t.BaseType)
                yield return t;
        }
    }
}
