using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace THNETII.WebServices.Authentication
{
    using static AuthenticationHandlerTypeHelpers;

    public static class AuthenticationSchemeSelector
    {
        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        public static async Task<string> ResolveSignInScheme(AuthenticationScheme authScheme, HttpContext httpContext, IAuthenticationHandlerProvider handlerProvider)
        {
            var (authName, handlerType) = authScheme;
            if (IsSignInHandler(handlerType))
                return authName;

            var handler = await GetHandler(httpContext, handlerProvider, authName);

            var options = GetHandlerOptions(handler, handlerType);
            return ResolveTarget(options?.ForwardSignIn, options, httpContext);
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        public static async Task<string> ResolveSignOutScheme(AuthenticationScheme authScheme, HttpContext httpContext, IAuthenticationHandlerProvider handlerProvider)
        {
            var (authName, handlerType) = authScheme;
            if (IsSignOutHandler(handlerType))
                return authName;

            var handler = await GetHandler(httpContext, handlerProvider, authName);

            var options = GetHandlerOptions(handler, handlerType);
            return ResolveTarget(options?.ForwardSignOut, options, httpContext);
        }

        [SuppressMessage("Code Quality", "IDE0051: Remove unused private members", Justification = "False positive")]
        private static void Deconstruct(this AuthenticationScheme authScheme, out string name, out Type handlerType)
            => (name, handlerType) = authScheme is null ? default : (authScheme.Name, authScheme.HandlerType);

        private static IAuthenticationHandlerProvider GetHandlerProvider(HttpContext httpContext, IAuthenticationHandlerProvider handlerProvider = null)
        {
            return handlerProvider ?? httpContext.RequestServices
                .GetRequiredService<IAuthenticationHandlerProvider>();
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        private static async Task<IAuthenticationHandler> GetHandler(HttpContext httpContext, IAuthenticationHandlerProvider handlerProvider, string authName)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            return await GetHandlerProvider(httpContext, handlerProvider)
                .GetHandlerAsync(httpContext, authName);
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        private static string ResolveTarget(string initialScheme, AuthenticationSchemeOptions options, HttpContext httpContext)
        {
            var resolvedScheme = initialScheme;
            if (!string.IsNullOrEmpty(resolvedScheme))
                return resolvedScheme;

            resolvedScheme = options?.ForwardDefaultSelector?.Invoke(httpContext);
            if (!string.IsNullOrEmpty(resolvedScheme))
                return resolvedScheme;

            resolvedScheme = options?.ForwardDefault;
            if (!string.IsNullOrEmpty(resolvedScheme))
                return resolvedScheme;

            return null;
        }
    }
}
