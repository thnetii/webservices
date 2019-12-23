using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace THNETII.WebServices.SampleRazorPages.MaterialDesignComponents.Pages
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
