using Microsoft.AspNetCore.Mvc;
using Stayora.Models;

namespace Stayora.ViewComponents
{
    public class _HotelFiltersComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke(HotelSearchResultViewModel model)
        {
            return View(model);
        }
    }
}