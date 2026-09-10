using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _LayoutHeaderComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
