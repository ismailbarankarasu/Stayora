using Stayora.Dtos.Booking;

namespace Stayora.Models
{
    public class HotelDetailsViewModel
    {
        public HotelSearchRequest Search { get; set; } = new();

        public HotelDetailsDto? Hotel { get; set; }

        public string? ErrorMessage { get; set; }
        public string? ReservationUrl { get; set; }
    }
}