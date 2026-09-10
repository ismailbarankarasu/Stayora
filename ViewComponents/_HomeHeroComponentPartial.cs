using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _HomeHeroComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}