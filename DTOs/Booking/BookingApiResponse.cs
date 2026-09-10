using System.Text.Json.Serialization;

namespace Stayora.Dtos.Booking
{
    public class BookingApiResponse<T>
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}