using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.StaticFiles;

using Xunit;

namespace THNETII.WebServices.StaticFiles.Registry.Test
{
    public static class ContentTypeProviderRegistryTest
    {
        [WindowsOSFact]
        public static void AddFromRegistry_on_null_provider_throws()
        {
            Assert.ThrowsAny<ArgumentNullException>(() => ((FileExtensionContentTypeProvider)null).AddFromRegistry());
        }

        [WindowsOSFact]
        public static void AddFromRegistry_adds_to_mappings()
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var provider = new FileExtensionContentTypeProvider(mappings);
            provider.AddFromRegistry();

            Assert.NotEmpty(provider.Mappings);
        }
    }
}
