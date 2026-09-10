using System.Text.Json.Serialization;

namespace Stayora.Dtos.Booking
{
    public class HotelFilterDataDto
    {
        [JsonPropertyName("filters")]
        public List<HotelFilterGroupDto> Filters { get; set; } = [];
    }

    public class HotelFilterGroupDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("filterStyle")]
        public string FilterStyle { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public List<HotelFilterOptionDto> Options { get; set; } = [];

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("min")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal? Min { get; set; }

        [JsonPropertyName("max")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal? Max { get; set; }

        [JsonPropertyName("minPriceStep")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal? MinPriceStep { get; set; }
    }

    public class HotelFilterOptionDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("genericId")]
        public string GenericId { get; set; } = string.Empty;

        [JsonPropertyName("countNotAutoextended")]
        public int? Count { get; set; }
    }
}