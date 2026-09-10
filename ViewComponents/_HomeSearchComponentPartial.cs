using Microsoft.AspNetCore.Mvc;
using Stayora.Models;

namespace Stayora.ViewComponents
{
    public class _HomeSearchComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke(HotelSearchRequest? search = null)
        {
            return View(search ?? new HotelSearchRequest());
        }
    }
}