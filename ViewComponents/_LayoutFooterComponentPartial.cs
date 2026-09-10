using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _LayoutFooterComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
