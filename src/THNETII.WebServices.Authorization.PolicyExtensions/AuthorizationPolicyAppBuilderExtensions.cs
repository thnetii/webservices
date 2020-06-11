using System;

using Microsoft.AspNetCore.Builder;

namespace THNETII.WebServices.Authorization.PolicyExtensions
{
    public static class AuthorizationPolicyAppBuilderExtensions
    {
        public static IApplicationBuilder UseAuthorizationPolicy(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<AuthorizationPolicyMiddleware>(new AuthorizationPolicyOptions
            {
                UseDefault = true,
            });
        }

        public static IApplicationBuilder UseAuthorizationPolicy(this IApplicationBuilder app, string policyName)
        {
            if (app is null)
                throw new ArgumentNullException(nameof(app));
            if (policyName is null)
                throw new ArgumentNullException(nameof(policyName));

            return app.UseMiddleware<AuthorizationPolicyMiddleware>(new AuthorizationPolicyOptions
            {
                UseDefault = false,
                PolicyName = policyName
            });
        }
    }
}
