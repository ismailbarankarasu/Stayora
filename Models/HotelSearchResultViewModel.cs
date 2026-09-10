using Stayora.Dtos.Booking;

namespace Stayora.Models
{
    public class HotelSearchResultViewModel
    {
        public HotelSearchRequest Search { get; set; } = new();

        public DestinationDto? Destination { get; set; }

        public List<HotelSearchItemDto> Hotels { get; set; } = [];

        public string? ErrorMessage { get; set; }
        public List<HotelFilterGroupDto> FilterGroups { get; set; } = [];

        public string? FilterErrorMessage { get; set; }
    }
}