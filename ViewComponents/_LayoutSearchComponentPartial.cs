using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _LayoutSearchComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
