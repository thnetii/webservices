using Microsoft.AspNetCore.Mvc;

namespace THNETII.WebServices.Claims.ViewComponents
{
    public class UserClaimsComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View(User);
        }
    }
}
