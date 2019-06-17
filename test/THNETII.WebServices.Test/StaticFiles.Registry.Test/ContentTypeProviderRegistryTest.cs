using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.AspNetCore.StaticFiles;

using Xunit;

namespace THNETII.WebServices.StaticFiles.Registry.Test
{
    public static class ContentTypeProviderRegistryTest
    {
        [Fact]
        public static void AddFromRegistry_on_null_provider_throws()
        {
            Assert.ThrowsAny<ArgumentNullException>(() => ((FileExtensionContentTypeProvider)null).AddFromRegistry());
        }

        [SkippableFact]
        public static void AddFromRegistry_adds_to_mappings()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "Registry is only supported on Windows operating systems.");
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var provider = new FileExtensionContentTypeProvider(mappings);
            provider.AddFromRegistry();

            Assert.NotEmpty(provider.Mappings);
        }
    }
}
