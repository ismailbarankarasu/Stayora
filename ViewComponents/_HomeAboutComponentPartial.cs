using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _HomeAboutComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}