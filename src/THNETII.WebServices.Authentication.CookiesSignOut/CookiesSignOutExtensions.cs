using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
                .GetRequestHandlerSchemesAsync())
                .ToDictionary(sch => sch.Name);
            var handlerProvider = httpContext.RequestServices
                .GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (var identity in user.Identities)
            {
                string remoteAuthSchemeName = identity.AuthenticationType;
                if (remoteAuthSchemeName == signOutContext.Scheme.Name ||
                    !remoteAuthSchemes.TryGetValue(remoteAuthSchemeName, out var remoteAuthScheme))
                    continue;

                var signOutScheme = await AuthenticationSchemeSelector
                    .ResolveSignOutScheme(remoteAuthScheme, httpContext, handlerProvider);
                if (string.IsNullOrEmpty(signOutScheme))
                    continue;

                await httpContext.SignOutAsync(signOutScheme, signOutContext.Properties);
            }
        }
    }
}
