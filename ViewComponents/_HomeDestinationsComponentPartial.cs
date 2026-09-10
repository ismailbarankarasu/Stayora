using Microsoft.AspNetCore.Mvc;

namespace Stayora.ViewComponents
{
    public class _HomeDestinationsComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}