using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace THNETII.WebServices.Authentication.CookiesSignOut
{
    public static class CookiesSignOutExtensions
    {
        public static AuthenticationBuilder AddCookieRemoteAuthSignOut(this AuthenticationBuilder builder)
        {
            builder.Services.PostConfigure<CookieAuthenticationOptions>(options =>
            {
                options.Events.OnSigningOut = OnCookieAuthenticationSignOut;
            });
            return builder;
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        private static async Task OnCookieAuthenticationSignOut(CookieSigningOutContext signOutContext)
        {
            var httpContext = signOutContext.HttpContext;
            var user = httpContext.User;
            var remoteAuthSchemes = (await httpContext.RequestServices
                .GetRequiredService<IAuthenticationSchemeProvider>()
                .GetRequestHandlerSchemesAsync().ConfigureAwait(false))
                .ToDictionary(sch => sch.Name);
            var handlerProvider = httpContext.RequestServices
                .GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (var identity in user.Identities)
            {
                string remoteAuthSchemeName = identity.AuthenticationType;
                if (remoteAuthSchemeName == signOutContext.Scheme.Name ||
                    !remoteAuthSchemes.TryGetValue(remoteAuthSchemeName, out var remoteAuthScheme))
                    continue;

                if (typeof(IAuthenticationSignOutHandler).IsAssignableFrom(remoteAuthScheme.HandlerType))
                {
                    await httpContext.SignOutAsync(remoteAuthScheme.Name);
                    continue;
                }

                var remoteOptionsType = GetHandlerOptionsType(remoteAuthScheme.HandlerType);
                if (remoteOptionsType is null)
                    continue;

                var remoteHandler = await handlerProvider.GetHandlerAsync(httpContext, remoteAuthScheme.Name);
                var remoteOptions = GetHandlerOptions(remoteHandler, remoteOptionsType);
                var remoteForwardSignOutScheme = ResolveTarget(remoteOptions.ForwardSignOut, remoteOptions, httpContext);
                if (string.IsNullOrEmpty(remoteForwardSignOutScheme))
                    continue;

                await httpContext.SignOutAsync(remoteForwardSignOutScheme, signOutContext.Properties);
            }
        }

        private delegate AuthenticationSchemeOptions GetHandlerOptionsFunc(IAuthenticationHandler handler);

        private static readonly Dictionary<Type, GetHandlerOptionsFunc> GetHandlerOptionsFuncs =
            new Dictionary<Type, GetHandlerOptionsFunc>();

        private static readonly MethodInfo GetHandlerOptionsGeneric =
            ((GetHandlerOptionsFunc)GetHandlerOptions<AuthenticationSchemeOptions>)
            .Method.GetGenericMethodDefinition();

        private static GetHandlerOptionsFunc CreateHandlerOptionsFunc(Type optionsType)
        {
            return (GetHandlerOptionsFunc)GetHandlerOptionsGeneric
                .MakeGenericMethod(optionsType)
                .CreateDelegate(typeof(GetHandlerOptionsFunc));
        }

        private static AuthenticationSchemeOptions GetHandlerOptions<TOptions>(IAuthenticationHandler handler)
            where TOptions : AuthenticationSchemeOptions, new()
        {
            if (handler is AuthenticationHandler<TOptions> handlerInstance)
                return handlerInstance.Options;
            return null;
        }

        private static AuthenticationSchemeOptions GetHandlerOptions(IAuthenticationHandler handler, Type optionsType)
        {
            GetHandlerOptionsFunc getOptions;
            lock (GetHandlerOptionsFuncs)
            {
                if (!GetHandlerOptionsFuncs.TryGetValue(optionsType, out getOptions))
                    GetHandlerOptionsFuncs[optionsType] = getOptions = CreateHandlerOptionsFunc(optionsType);
            }
            return getOptions(handler);
        }

        private static Type GetHandlerOptionsType(Type handlerType)
        {
            var authHandlerType = EnumerateTypeHierarchy(handlerType)
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(AuthenticationHandler<>));
            if (authHandlerType is null)
                return null;

            return authHandlerType.GetGenericArguments()[0];
        }

        private static IEnumerable<Type> EnumerateTypeHierarchy(Type type)
        {
            for (var t = type; t is Type; t = t.BaseType)
                yield return t;
        }

        private static string ResolveTarget(string forwardScheme, AuthenticationSchemeOptions options, HttpContext httpContext)
        {
            if (!string.IsNullOrEmpty(forwardScheme))
                goto returnForwardScheme;

            forwardScheme = options.ForwardDefaultSelector?.Invoke(httpContext);
            if (!string.IsNullOrEmpty(forwardScheme))
                goto returnForwardScheme;

            forwardScheme = options.ForwardDefault;
            if (!string.IsNullOrEmpty(forwardScheme))
                goto returnForwardScheme;

            return null;

        returnForwardScheme:
            return forwardScheme;
        }
    }
}
