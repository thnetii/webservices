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
            if (IsSignInHandler(authScheme?.HandlerType))
                return authScheme.Name;

            var handler = await (handlerProvider ?? httpContext.RequestServices
                .GetRequiredService<IAuthenticationHandlerProvider>())
                .GetHandlerAsync(httpContext, authScheme?.Name);

            var options = GetHandlerOptions(handler, authScheme?.HandlerType);
            return ResolveTarget(options?.ForwardSignIn, options, httpContext);
        }

        [SuppressMessage("Reliability", "CA2007: Do not directly await a Task")]
        public static async Task<string> ResolveSignOutScheme(AuthenticationScheme authScheme, HttpContext httpContext, IAuthenticationHandlerProvider handlerProvider)
        {
            if (IsSignOutHandler(authScheme?.HandlerType))
                return authScheme.Name;

            var handler = await (handlerProvider ?? httpContext.RequestServices
                .GetRequiredService<IAuthenticationHandlerProvider>())
                .GetHandlerAsync(httpContext, authScheme?.Name);

            var options = GetHandlerOptions(handler, authScheme?.HandlerType);
            return ResolveTarget(options?.ForwardSignOut, options, httpContext);
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
