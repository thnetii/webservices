using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace THNETII.WebServices.Authorization.PolicyExtensions
{
    public class AuthorizationPolicyMiddleware
    {
        private readonly IAuthorizationPolicyProvider policyProvider;
        private readonly RequestDelegate next;
        private readonly AuthorizationPolicyOptions options;

        public AuthorizationPolicyMiddleware(IAuthorizationPolicyProvider policyProvider, RequestDelegate next, AuthorizationPolicyOptions options)
        {
            this.policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var (useDefaultPolicy, policyName) = options is null
                ? (true, null)
                : (options.UseDefault, options.PolicyName);

            var policy = await (useDefaultPolicy
                ? policyProvider.GetDefaultPolicyAsync()
                : policyProvider.GetPolicyAsync(policyName));

            // Policy evaluator has transient lifetime so it fetched from request services instead of injecting in constructor
            var policyEvaluator = context.RequestServices.GetRequiredService<IPolicyEvaluator>();

            var authenticateResult = await policyEvaluator.AuthenticateAsync(policy, context);

            var authorizeResult = await policyEvaluator.AuthorizeAsync(policy, authenticateResult, context, resource: null);

            if (authorizeResult.Challenged)
            {
                if (policy.AuthenticationSchemes.Any())
                {
                    foreach (var scheme in policy.AuthenticationSchemes)
                    {
                        await context.ChallengeAsync(scheme);
                    }
                }
                else
                {
                    await context.ChallengeAsync();
                }

                return;
            }
            else if (authorizeResult.Forbidden)
            {
                if (policy.AuthenticationSchemes.Any())
                {
                    foreach (var scheme in policy.AuthenticationSchemes)
                    {
                        await context.ForbidAsync(scheme);
                    }
                }
                else
                {
                    await context.ForbidAsync();
                }

                return;
            }

            await (next?.Invoke(context) ?? Task.CompletedTask);
        }
    }
}
