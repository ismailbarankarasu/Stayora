using System.Text.Json.Serialization;

namespace Stayora.Dtos.Booking
{
    public class HotelSortOptionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }
}