using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Win32;

using Win32Registry = Microsoft.Win32.Registry;

namespace THNETII.WebServices.StaticFiles.Registry
{
    [SuppressMessage("Usage", "PC001: API not supported on all platforms", Justification = "Windows-only API")]
    public static class ContentTypeProviderRegistryExtensions
    {
        public static void AddFromRegistry(this FileExtensionContentTypeProvider provider, bool preserveExisting = false)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            provider?.Mappings.AddFromRegistryClassesRoot(Win32Registry.ClassesRoot, preserveExisting);
        }

        private static void AddFromRegistryClassesRoot(this IDictionary<string, string> mappings, RegistryKey classesRoot, bool preserveExisting = false)
        {
            foreach (var ext in classesRoot.GetSubKeyNames().Where(k => k.StartsWith(".", StringComparison.Ordinal)))
            {
                using (var key = classesRoot.OpenSubKey(ext))
                {
                    mappings.AddFromRegistryKey(ext, key, preserveExisting);
                }
            }
        }

        private static void AddFromRegistryKey(this IDictionary<string, string> mappings, string ext, RegistryKey key, bool preserveExisting = false)
        {
            if (preserveExisting && mappings.ContainsKey(ext))
                return;
            if (key.GetValue("Content Type") is string contentType)
            {
                mappings[ext] = contentType;
            }
        }
    }
}
