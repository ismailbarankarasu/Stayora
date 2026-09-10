using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _LayoutOffcanvasComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
