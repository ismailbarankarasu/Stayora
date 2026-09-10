using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Stayora.Dtos.Booking;
using Stayora.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Stayora.Services
{
    public class BookingService : IBookingService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public BookingService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<HotelFilterDataDto> GetFiltersAsync(DestinationDto destination, HotelSearchRequest request, CancellationToken cancellationToken = default)
        {
            Validator.ValidateObject(
                request,
                new ValidationContext(request),
                validateAllProperties: true);

            if (string.IsNullOrWhiteSpace(destination.DestinationId) ||
                string.IsNullOrWhiteSpace(destination.SearchType))
            {
                throw new ArgumentException(
                    "Geçerli bir destinasyon seçilmelidir.",
                    nameof(destination));
            }

            var parameters = new Dictionary<string, string?>
            {
                ["dest_id"] = destination.DestinationId,

                ["search_type"] =
                    destination.SearchType.ToUpperInvariant(),

                ["arrival_date"] = request.CheckIn!.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["departure_date"] = request.CheckOut!.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["adults"] = request.Adults.ToString(
                    CultureInfo.InvariantCulture),

                ["room_qty"] = request.Rooms.ToString(
                    CultureInfo.InvariantCulture)
            };

            var url = QueryHelpers.AddQueryString(
                "api/v1/hotels/getFilter",
                parameters);

            var cacheKey = $"booking:filters:{url}";

            if (_cache.TryGetValue(
                    cacheKey,
                    out HotelFilterDataDto? cachedFilters)
                && cachedFilters is not null)
            {
                return cachedFilters;
            }

            var filters = await GetDataAsync<HotelFilterDataDto>(
                url, cancellationToken);

            _cache.Set(
                cacheKey,
                filters,
                TimeSpan.FromMinutes(2));

            return filters;
        }

        public async Task<HotelDetailsDto> GetHotelDetailsAsync(long hotelId, HotelSearchRequest request, CancellationToken cancellationToken = default)
        {
            if (hotelId <= 0)
            {
                throw new ArgumentException(
                    "Geçerli bir otel seçilmelidir.", nameof(hotelId));
            }

            Validator.ValidateObject(
                request,
                new ValidationContext(request),
                validateAllProperties: true);

            var parameters = new Dictionary<string, string?>
            {
                ["hotel_id"] = hotelId.ToString(
                    CultureInfo.InvariantCulture),

                ["arrival_date"] = request.CheckIn!.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["departure_date"] = request.CheckOut!.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["adults"] = request.Adults.ToString(
                    CultureInfo.InvariantCulture),

                ["room_qty"] = request.Rooms.ToString(
                    CultureInfo.InvariantCulture),

                ["languagecode"] = "en-us",
                ["currency_code"] = "TRY"
            };

            var url = QueryHelpers.AddQueryString(
                "api/v1/hotels/getHotelDetails",
                parameters);

            return await GetDataAsync<HotelDetailsDto>(
                url, cancellationToken);
        }

        public async Task<List<DestinationDto>> SearchDestinationsAsync(string city, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                throw new ArgumentException(
                    "Şehir bilgisi boş olamaz.", nameof(city));
            }

            var url = QueryHelpers.AddQueryString(
                "api/v1/hotels/searchDestination",
                "query",
                city.Trim());

            var cacheKey = $"booking:destinations:{url}";

            if (_cache.TryGetValue(
                    cacheKey,
                    out List<DestinationDto>? cachedDestinations)
                && cachedDestinations is not null)
            {
                return cachedDestinations;
            }

            var destinations = await GetDataAsync<List<DestinationDto>>(
                url, cancellationToken);

            _cache.Set(
                cacheKey,
                destinations,
                TimeSpan.FromHours(1));

            return destinations;
        }

        public async Task<HotelSearchDataDto> SearchHotelsAsync(DestinationDto destination, HotelSearchRequest request, CancellationToken cancellationToken = default)
        {
            Validator.ValidateObject(
                request,
                new ValidationContext(request),
                validateAllProperties: true);

            if (string.IsNullOrWhiteSpace(destination.DestinationId) ||
                string.IsNullOrWhiteSpace(destination.SearchType))
            {
                throw new ArgumentException(
                    "Geçerli bir destinasyon seçilmelidir.",
                    nameof(destination));
            }

            const int apiPageSize = 20;
            const int displayPageSize = 4;

            var firstItemIndex =
                ((long)request.PageNumber - 1) * displayPageSize;

            var apiPageNumber = firstItemIndex / apiPageSize + 1;
            var skipCount = (int)(firstItemIndex % apiPageSize);

            var parameters = new Dictionary<string, string?>
            {
                ["dest_id"] = destination.DestinationId,

                ["search_type"] =
                    destination.SearchType.ToUpperInvariant(),

                ["arrival_date"] = request.CheckIn!.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["departure_date"] = request.CheckOut!.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["adults"] = request.Adults.ToString(
                    CultureInfo.InvariantCulture),

                ["room_qty"] = request.Rooms.ToString(
                    CultureInfo.InvariantCulture),

                ["page_number"] = apiPageNumber.ToString(
                    CultureInfo.InvariantCulture),

                ["languagecode"] = "en-us",
                ["currency_code"] = "TRY",
                ["location"] = "US",
                ["sort_by"] = request.SortBy
            };

            if (!string.IsNullOrWhiteSpace(request.CategoryFilter))
            {
                parameters["categories_filter"] = request.CategoryFilter;
            }

            if (request.MinPrice.HasValue)
            {
                parameters["price_min"] = request.MinPrice.Value.ToString(
                    CultureInfo.InvariantCulture);
            }

            if (request.MaxPrice.HasValue)
            {
                parameters["price_max"] = request.MaxPrice.Value.ToString(
                    CultureInfo.InvariantCulture);
            }

            var url = QueryHelpers.AddQueryString(
                "api/v1/hotels/searchHotels",
                parameters);
            var cacheKey = $"booking:hotels:{url}";

            if (!_cache.TryGetValue(
                    cacheKey,
                    out HotelSearchDataDto? apiResult)
                || apiResult is null)
            {
                apiResult = await GetDataAsync<HotelSearchDataDto>(
                    url, cancellationToken);

                _cache.Set(
                    cacheKey,
                    apiResult,
                    TimeSpan.FromMinutes(2));
            }

            var hotels = apiResult.Hotels
                .Skip(skipCount)
                .Take(displayPageSize)
                .ToList();

            var hasMoreInCurrentBatch = apiResult.Hotels.Count > skipCount + displayPageSize;

            var mightHaveNextApiPage = apiResult.Hotels.Count == apiPageSize;

            return new HotelSearchDataDto
            {
                Hotels = hotels,

                Meta = apiResult.Meta,

                HasNextPage = hasMoreInCurrentBatch || mightHaveNextApiPage
            };
        }

        private async Task<T> GetDataAsync<T>(string url, CancellationToken cancellationToken) where T : class
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<BookingApiResponse<T>>(
                    cancellationToken: cancellationToken);

            if (result is null)
            {
                throw new InvalidOperationException(
                    "Booking API cevabı okunamadı.");
            }

            if (!result.Status)
            {
                throw new InvalidOperationException(
                    "Booking API isteği başarısız sonuçlandı.");
            }

            return result.Data
                ?? throw new InvalidOperationException(
                    "Booking API cevabında data alanı bulunamadı.");
        }
    }

}