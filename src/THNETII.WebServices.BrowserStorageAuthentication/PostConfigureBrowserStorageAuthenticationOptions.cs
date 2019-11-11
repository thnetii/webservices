using System;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace THNETII.WebServices.BrowserStorageAuthentication
{
    public class PostConfigureBrowserStorageAuthenticationOptions
        : IPostConfigureOptions<BrowserStorageAuthenticationOptions>
    {
        private readonly IDataProtectionProvider dataProtection;

        public PostConfigureBrowserStorageAuthenticationOptions(
            IDataProtectionProvider dataProtection)
        {
            this.dataProtection = dataProtection;
        }

        public void PostConfigure(string name,
            BrowserStorageAuthenticationOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            options.DataProtectionProvider ??= dataProtection;

            if (options.TicketDataFormat == null)
            {
                var dataProtector = options.DataProtectionProvider
                    .CreateProtector(
                        typeof(BrowserStorageAuthenticationHandler).FullName,
                        name
                        );
                options.TicketDataFormat = new TicketDataFormat(dataProtector);
            }
        }
    }
}
