using Stayora.Dtos.Booking;
using Stayora.Models;

namespace Stayora.Services
{
    public interface IBookingService
    {
        Task<List<DestinationDto>> SearchDestinationsAsync(string city, CancellationToken cancellationToken = default);
        Task<HotelSearchDataDto> SearchHotelsAsync(DestinationDto destination, HotelSearchRequest request, CancellationToken cancellationToken = default);
        Task<HotelDetailsDto> GetHotelDetailsAsync(long hotelId, HotelSearchRequest request, CancellationToken cancellationToken = default);
        Task<HotelFilterDataDto> GetFiltersAsync(DestinationDto destination, HotelSearchRequest request, CancellationToken cancellationToken = default);
    }
}