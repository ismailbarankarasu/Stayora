using System.Text.Json.Serialization;

namespace Stayora.Dtos.Booking
{
    public class HotelSearchDataDto
    {
        [JsonPropertyName("hotels")]
        public List<HotelSearchItemDto> Hotels { get; set; } = [];

        [JsonPropertyName("meta")]
        public List<HotelSearchMetaDto> Meta { get; set; } = [];
    }

    public class HotelSearchItemDto
    {
        [JsonPropertyName("hotel_id")]
        public long HotelId { get; set; }

        [JsonPropertyName("accessibilityLabel")]
        public string? AccessibilityLabel { get; set; }

        [JsonPropertyName("property")]
        public HotelPropertyDto? Property { get; set; }
    }

    public class HotelPropertyDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("wishlistName")]
        public string? DestinationName { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("photoUrls")]
        public List<string> PhotoUrls { get; set; } = [];

        [JsonPropertyName("reviewScore")]
        public double? ReviewScore { get; set; }

        [JsonPropertyName("reviewScoreWord")]
        public string? ReviewScoreWord { get; set; }

        [JsonPropertyName("reviewCount")]
        public int? ReviewCount { get; set; }

        [JsonPropertyName("propertyClass")]
        public int? PropertyClass { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("checkinDate")]
        public string? CheckInDate { get; set; }

        [JsonPropertyName("checkoutDate")]
        public string? CheckOutDate { get; set; }

        [JsonPropertyName("priceBreakdown")]
        public HotelPriceBreakdownDto? PriceBreakdown { get; set; }
    }

    public class HotelPriceBreakdownDto
    {
        [JsonPropertyName("grossPrice")]
        public HotelMoneyDto? GrossPrice { get; set; }

        [JsonPropertyName("strikethroughPrice")]
        public HotelMoneyDto? StrikethroughPrice { get; set; }

        [JsonPropertyName("excludedPrice")]
        public HotelMoneyDto? ExcludedPrice { get; set; }

        [JsonPropertyName("benefitBadges")]
        public List<HotelBenefitBadgeDto> BenefitBadges { get; set; } = [];
    }

    public class HotelMoneyDto
    {
        [JsonPropertyName("value")]
        public decimal? Value { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }

    public class HotelBenefitBadgeDto
    {
        [JsonPropertyName("identifier")]
        public string? Identifier { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }
    }

    public class HotelSearchMetaDto
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}