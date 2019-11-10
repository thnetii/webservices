using System.Runtime.InteropServices;

using Xunit;

namespace THNETII.WebServices
{
    public class WindowsOSFactAttribute : FactAttribute
    {
        public WindowsOSFactAttribute()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Skip = "Fact is only testable on Windows OS Platforms.";
            }
        }
    }
}
