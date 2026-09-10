using System.Text.Json.Serialization;

namespace Stayora.Dtos.Booking
{
    public class DestinationDto
    {
        [JsonPropertyName("dest_id")]
        public string DestinationId { get; set; } = string.Empty;

        [JsonPropertyName("search_type")]
        public string SearchType { get; set; } = string.Empty;

        [JsonPropertyName("dest_type")]
        public string DestinationType { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("city_name")]
        public string? CityName { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("nr_hotels")]
        public int? HotelCount { get; set; }
    }
}