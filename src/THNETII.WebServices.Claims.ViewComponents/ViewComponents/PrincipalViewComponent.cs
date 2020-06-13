
using System.Security.Principal;

using Microsoft.AspNetCore.Mvc;

namespace THNETII.WebServices.Claims.ViewComponents
{
    [ViewComponent]
    public class PrincipalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IPrincipal principal) =>
            View(principal);
    }
}
