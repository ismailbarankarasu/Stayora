namespace Stayora.Models.Chat
{
    public class ChatHotelCard
    {
        public long HotelId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public double? ReviewScore { get; set; }

        public int? ReviewCount { get; set; }

        public decimal? Price { get; set; }

        public string? Currency { get; set; }

        public decimal? ExcludedPrice { get; set; }

        public string? ExcludedPriceCurrency { get; set; }

        public List<string> PriceConditions { get; set; } = [];

        public string DetailsUrl { get; set; } = string.Empty;
    }
}