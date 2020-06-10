using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;

namespace THNETII.WebServices.RazorPagesTemplate.Bootstrap.Pages
{
    public class PrivacyModel : PageModel
    {
        [SuppressMessage("Code Quality", "IDE0052: Remove unread private members")]
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        [SuppressMessage("Performance", "CA1822: Mark members as static")]
        public void OnGet()
        {
        }
    }
}
