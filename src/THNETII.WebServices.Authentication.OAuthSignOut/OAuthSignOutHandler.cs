using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
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

        protected IAuthenticationRequestHandler OAuthHandler { get; set; }
        protected OAuthOptions OAuthOptions { get; set; }

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

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected override async Task InitializeHandlerAsync()
        {
            await base.InitializeHandlerAsync();

            var oauthScheme = await SchemeProvider.GetSchemeAsync(Options.RemoteAuthenticationScheme);
            string oauthSchemeName;
            Type oauthHandlerType;
            if (oauthScheme is null)
                (oauthSchemeName, oauthHandlerType) = (null, null);
            else
                (oauthSchemeName, oauthHandlerType) = (oauthScheme.Name, oauthScheme.HandlerType);
            Type oauthHandlerImpl;
            for (oauthHandlerImpl = oauthHandlerType; oauthHandlerImpl is Type; oauthHandlerImpl = oauthHandlerImpl.BaseType)
            {
                if (oauthHandlerImpl.IsGenericType && oauthHandlerImpl.GetGenericTypeDefinition() == typeof(OAuthHandler<>))
                    break;
            }
            if (oauthHandlerImpl is null)
            {
                throw new InvalidOperationException($"Authentication Handler type '{oauthHandlerType}' registered for remote authentication scheme '{oauthScheme}' does not implement the '{typeof(OAuthHandler<>)}' base class.");
            }

            var oauthHandler = await HandlerProvider.GetHandlerAsync(Context, oauthSchemeName);
            var oauthOptions = AuthenticationHandlerTypeHelpers
                .GetHandlerOptions(oauthHandler, oauthHandlerType);

            OAuthHandler = (IAuthenticationRequestHandler)oauthHandler;
            OAuthOptions = (OAuthOptions)oauthOptions;
        }

        protected override Task<object> CreateEventsAsync() =>
            Task.FromResult<object>(new OAuthSignOutEvents());

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => OAuthHandler.AuthenticateAsync();

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected sealed override async Task HandleSignOutAsync(AuthenticationProperties properties)
        {
            string oauthSchemeName = Options.RemoteAuthenticationScheme;

            var authResult = await OAuthHandler.AuthenticateAsync();
            if (!authResult.Succeeded)
                return;

            var authTicket = authResult.Ticket;

            var signoutContext = new OAuthSignOutContext(Context, Scheme, Options, properties, OAuthHandler, authTicket);

            await Events.SigningOut(signoutContext);

            foreach (var token in properties.GetTokens().Where(ShouldRevokeToken))
            {
                var revokeContext = new OAuthRevokeTokenContext(
                    Context, Scheme, Options, properties, token,
                    OAuthHandler);

                await Events.RevokeToken(revokeContext);
                try
                {
                    await RevokeTokenAsync(revokeContext);
                    Logger.TokenRevoked(token.Name, oauthSchemeName);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception revokeExcept)
                {
                    Logger.TokenRevokeFailure(token.Name, oauthSchemeName, revokeExcept);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            Logger.SignedOut(oauthSchemeName);

            string redirectUri = properties?.RedirectUri;
            if (!string.IsNullOrEmpty(redirectUri))
                Context.Response.Redirect(redirectUri);
        }

        protected virtual bool ShouldRevokeToken(AuthenticationToken token)
        {
            switch (token?.Name)
            {
                case "access_token":
                case "refresh_token":
                    return true;
                default:
                    return false;
            }
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        protected virtual async Task RevokeTokenAsync(OAuthRevokeTokenContext context)
        {
            var options = OAuthOptions;
            var revokeEndpoint = GetRevokeEndpoint();
            if (revokeEndpoint is null)
                return;
            using (var requestContent = GetRevokeRequestContent(context?.Token))
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, revokeEndpoint)
            {
                Content = requestContent
            })
            using (var response = await options.Backchannel.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        protected virtual string GetRevokeEndpoint() =>
            Options.RevokeEndpoint ?? Options.RevokeEndpointFallback?.Invoke(OAuthOptions);

        protected virtual HttpContent GetRevokeRequestContent(AuthenticationToken token)
        {
            if (token is null)
                return null;
            return new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token.Value,
                ["token_type_hint"] = token.Name
            });
        }
    }
}
