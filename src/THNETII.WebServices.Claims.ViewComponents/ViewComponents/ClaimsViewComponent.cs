
using Microsoft.AspNetCore.Mvc;

namespace THNETII.WebServices.Claims.ViewComponents
{
    [ViewComponent]
    public class ClaimsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
