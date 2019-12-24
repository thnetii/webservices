using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;

namespace THNETII.WebServices.RazorPagesTemplate.MaterialDesignComponents.Pages
{
    public class IndexModel : PageModel
    {
        [SuppressMessage("Code Quality", "IDE0052: Remove unread private members")]
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [SuppressMessage("Performance", "CA1822: Mark members as static")]
        public void OnGet()
        {

        }
    }
}
