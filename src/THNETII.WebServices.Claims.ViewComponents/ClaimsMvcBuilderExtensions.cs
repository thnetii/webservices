using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace THNETII.WebServices.Claims
{
    public static class ClaimsMvcBuilderExtensions
    {
        public static IMvcBuilder AddClaimsApplicationPart(this IMvcBuilder mvc)
        {
            _ = mvc ?? throw new ArgumentNullException(nameof(mvc));

            mvc.AddApplicationPart(typeof(ClaimsMvcBuilderExtensions).Assembly);

            return mvc;
        }
    }
}
