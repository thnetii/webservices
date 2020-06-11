using System;
using System.Threading.Tasks;

namespace THNETII.WebServices.Authentication.OAuthSignOut
{
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
}
