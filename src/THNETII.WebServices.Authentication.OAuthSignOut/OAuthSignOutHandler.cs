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

            var oauthOptionType = GetOAuthOptionsType(remoteHandler?.GetType());
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

                        await RevokeTokenAsync(oauthHandler, token);
                        break;
                }
            }

            Logger.SignedOut(oauthHandler.Scheme.Name);
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected virtual async Task RevokeTokenAsync<TOAuthOptions>(
            OAuthHandler<TOAuthOptions> oauthHandler, AuthenticationToken token)
            where TOAuthOptions : OAuthOptions, new()
        {
            var revokeEndpoint = GetRevokeEndpoint(oauthHandler.Options);
            if (revokeEndpoint is null)
                return;
            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token.Value,
                ["token_type_hint"] = token.Name
            });
            using (requestContent)
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, revokeEndpoint)
            {
                Content = requestContent
            })
            using (var response = await oauthHandler.Options.Backchannel.SendAsync(requestMessage, Context.RequestAborted))
            {
                if (response.IsSuccessStatusCode)
                    Logger.TokenRevoked(token.Name, oauthHandler.Scheme.Name);
                else
                    Logger.TokenRevokeFailure(token.Name, oauthHandler.Scheme.Name, response);
            }
        }

        protected virtual string GetRevokeEndpoint(OAuthOptions oAuthOptions) =>
            Options.RevokeEndpoint ?? Options.RevokeEndpointFallback?.Invoke(oAuthOptions);

        private static Type GetOAuthOptionsType(Type handlerType)
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
    }

    public static class OAuthSignOutDefaults
    {
        public static string RevokeEndpointFallback(OAuthOptions options)
        {
            try
            {
                var authorizeUri = new Uri(options.AuthorizationEndpoint);
                return new Uri(authorizeUri, "revoke").ToString();
            }
            catch (UriFormatException) { return null; }
        }

        public static readonly string AuthenticationSchemeSuffix = "-SignOut";

        public static readonly string DisplayName = OAuthDefaults.DisplayName;

        public static string AuthenticationScheme(string oauthScheme) =>
            oauthScheme + AuthenticationSchemeSuffix;
    }

    internal static class OAuthSignOutLoggerExtensions
    {
        private static readonly Action<ILogger, string, string, Exception> tokenRevoked =
            LoggerMessage.Define<string, string>(
                LogLevel.Information, new EventId(21, "OAuthTokenRevoked"),
                "OAuthToken '{TokenName}' was revoked for authentication scheme '{OAuthScheme}'");

        private static readonly Action<ILogger, string, string, int, string, Exception> tokenRevokeFailure =
            LoggerMessage.Define<string, string, int, string>(
                LogLevel.Warning, new EventId(22, "OAuthTokenRevokeFailure"),
                "OAuthToken '{TokenName}' revocation for authentication scheme '{OAuthScheme}' failed. {StatusCode} {ReasonPhrase}");

        private static readonly Action<ILogger, string, Exception> signedOut =
            LoggerMessage.Define<string>(
                LogLevel.Debug, new EventId(20, "OAuthSignedOut"),
                "AuthenticationScheme: {AuthenticationScheme} was signed out.");

        public static void TokenRevoked(this ILogger logger, string tokenName, string oauthScheme) =>
            tokenRevoked(logger, tokenName, oauthScheme, null);

        public static void TokenRevokeFailure(this ILogger logger, string tokenName, string oauthScheme, HttpResponseMessage response) =>
            tokenRevokeFailure(logger, tokenName, oauthScheme, (int)response.StatusCode, response.ReasonPhrase, null);

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
}
