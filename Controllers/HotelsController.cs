using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stayora.Models;
using Stayora.Services;
using System.Globalization;
using System.Text.Json;

namespace Stayora.Controllers
{
    public class HotelsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(
            IBookingService bookingService,
            ILogger<HotelsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] HotelSearchRequest request, CancellationToken cancellationToken)
        {
            var model = new HotelSearchResultViewModel
            {
                Search = request
            };

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var destinations =
                    await _bookingService.SearchDestinationsAsync(
                        request.City, cancellationToken);

                var destination = destinations
                    .Take(2)
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.SearchType,
                            "city",
                            StringComparison.OrdinalIgnoreCase));

                if (destination is null)
                {
                    model.ErrorMessage =
                        "Bu arama için şehir bulunamadı. " +
                        "Lütfen şehir adını daha açık yazarak tekrar deneyin.";

                    return View(model);
                }

                model.Destination = destination;

                var result = await _bookingService.SearchHotelsAsync(
                    destination,
                    request,
                    cancellationToken);

                model.Hotels = result.Hotels
                    .Where(x => x.Property is not null)
                    .ToList();
                try
                {
                    var filterData = await _bookingService.GetFiltersAsync(
                        destination,
                        request,
                        cancellationToken);

                    var visibleFields = new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase)
                        {"class", "review_score", "hotelfacility", "mealplan", "fc", "ht_id" };

                    model.FilterGroups = filterData.Filters
                        .Where(group =>
                            visibleFields.Contains(group.Field) &&
                            group.Options.Count > 0)
                        .ToList();
                }
                catch (OperationCanceledException)
                    when (!cancellationToken.IsCancellationRequested)
                {
                    model.FilterErrorMessage =
                        "Ek filtreler zamanında yüklenemedi. " +
                        "Otel sonuçlarını incelemeye devam edebilirsiniz.";
                }
                catch (Exception exception)
                    when (exception is HttpRequestException
                        or JsonException
                        or InvalidOperationException)
                {
                    _logger.LogWarning(
                        exception,
                        "Otel filtreleri yüklenemedi.");

                    model.FilterErrorMessage =
                        "Ek filtreler şu anda yüklenemiyor. " +
                        "Otel sonuçlarını incelemeye devam edebilirsiniz.";
                }
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                model.ErrorMessage =
                    "Otel servisi zamanında yanıt vermedi. " +
                    "Lütfen tekrar deneyin.";
            }
            catch (Exception exception)
                when (exception is HttpRequestException
                    or JsonException
                    or InvalidOperationException)
            {
                _logger.LogError(
                    exception,
                    "Otel araması başarısız oldu.");

                model.ErrorMessage =
                    "Oteller şu anda yüklenemiyor. " +
                    "Lütfen biraz sonra tekrar deneyin.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long hotelId, [FromQuery] HotelSearchRequest request, CancellationToken cancellationToken)
        {
            if (hotelId <= 0)
            {
                return BadRequest("Geçersiz otel bilgisi.");
            }

            var model = new HotelDetailsViewModel
            {
                Search = request
            };

            if (!ModelState.IsValid)
            {
                model.ErrorMessage =
                    "Arama bilgileri eksik veya geçersiz. " +
                    "Lütfen yeniden otel araması yapın.";

                return View(model);
            }

            try
            {
                model.Hotel = await _bookingService.GetHotelDetailsAsync(
                    hotelId,
                    request,
                    cancellationToken);
                model.ReservationUrl = BuildReservationUrl(model.Hotel.BookingUrl, request);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                model.ErrorMessage =
                    "Otel servisi zamanında yanıt vermedi. " +
                    "Lütfen tekrar deneyin.";
            }
            catch (Exception exception)
                when (exception is HttpRequestException
                    or JsonException
                    or InvalidOperationException)
            {
                _logger.LogError(
                    exception,
                    "Otel detayı yüklenemedi. HotelId: {HotelId}",
                    hotelId);

                model.ErrorMessage =
                    "Otel detayları şu anda yüklenemiyor. " +
                    "Lütfen biraz sonra tekrar deneyin.";
            }

            return View(model);
        }
        private static string? BuildReservationUrl(
    string? hotelUrl,
    HotelSearchRequest request)
        {
            if (!Uri.TryCreate(hotelUrl, UriKind.Absolute, out var uri))
            {
                return null;
            }

            // Yalnızca Booking.com otel bağlantılarına izin ver.
            var isBookingHost =
                uri.Host.Equals(
                    "booking.com",
                    StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(
                    ".booking.com",
                    StringComparison.OrdinalIgnoreCase);

            if (uri.Scheme != Uri.UriSchemeHttps ||
                !isBookingHost ||
                !uri.IsDefaultPort ||
                !string.IsNullOrEmpty(uri.UserInfo) ||
                !uri.AbsolutePath.StartsWith(
                    "/hotel/",
                    StringComparison.OrdinalIgnoreCase) ||
                !request.CheckIn.HasValue ||
                !request.CheckOut.HasValue)
            {
                return null;
            }
            var baseUrl = uri.GetLeftPart(UriPartial.Path);

            var parameters = new Dictionary<string, string?>
            {
                ["checkin"] = request.CheckIn.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["checkout"] = request.CheckOut.Value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture),

                ["group_adults"] = request.Adults.ToString(
                    CultureInfo.InvariantCulture),

                ["group_children"] = "0",

                ["no_rooms"] = request.Rooms.ToString(
                    CultureInfo.InvariantCulture),

                ["selected_currency"] = "AED"
            };

            return QueryHelpers.AddQueryString(baseUrl, parameters);
        }
    }
}
