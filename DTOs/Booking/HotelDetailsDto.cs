using System.Text.Json.Serialization;

namespace Stayora.Dtos.Booking
{
    public class HotelDetailsDto
    {
        [JsonPropertyName("hotel_id")]
        public long HotelId { get; set; }

        [JsonPropertyName("hotel_name")]
        public string HotelName { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string? BookingUrl { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("country_trans")]
        public string? Country { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("review_nr")]
        public int? ReviewCount { get; set; }

        [JsonPropertyName("arrival_date")]
        public string? ArrivalDate { get; set; }

        [JsonPropertyName("departure_date")]
        public string? DepartureDate { get; set; }

        [JsonPropertyName("soldout")]
        public int? SoldOut { get; set; }

        [JsonPropertyName("facilities_block")]
        public HotelFacilitiesBlockDto? FacilitiesBlock { get; set; }

        [JsonPropertyName("product_price_breakdown")]
        public HotelDetailPriceDto? PriceBreakdown { get; set; }

        [JsonPropertyName("rooms")]
        public Dictionary<string, HotelDetailRoomDto> Rooms { get; set; } = [];
    }

    public class HotelFacilitiesBlockDto
    {
        [JsonPropertyName("facilities")]
        public List<HotelFacilityDto> Facilities { get; set; } = [];
    }

    public class HotelFacilityDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
    }

    public class HotelDetailPriceDto
    {
        [JsonPropertyName("all_inclusive_amount")]
        public HotelMoneyDto? AllInclusiveAmount { get; set; }

        [JsonPropertyName("excluded_amount")]
        public HotelMoneyDto? ExcludedAmount { get; set; }

        [JsonPropertyName("gross_amount_hotel_currency")]
        public HotelMoneyDto? GrossAmountHotelCurrency { get; set; }

        [JsonPropertyName("benefits")]
        public List<HotelDetailBenefitDto> Benefits { get; set; } = [];
    }

    public class HotelDetailBenefitDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("details")]
        public string? Details { get; set; }
    }

    public class HotelDetailRoomDto
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("photos")]
        public List<HotelDetailPhotoDto> Photos { get; set; } = [];
    }

    public class HotelDetailPhotoDto
    {
        [JsonPropertyName("photo_id")]
        public long PhotoId { get; set; }

        [JsonPropertyName("url_max1280")]
        public string? LargeUrl { get; set; }

        [JsonPropertyName("url_original")]
        public string? OriginalUrl { get; set; }

        [JsonPropertyName("url_square180")]
        public string? ThumbnailUrl { get; set; }
    }
}