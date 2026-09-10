using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _HomeHowItWorksComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}