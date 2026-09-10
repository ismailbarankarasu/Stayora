namespace Stayora.Models.Chat
{
    public class ChatHotelSearchCriteria
    {
        public string? City { get; set; }

        public DateOnly? CheckIn { get; set; }

        public DateOnly? CheckOut { get; set; }

        public int? Adults { get; set; }

        public int? Rooms { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? CategoryFilter { get; set; }

        public string? SortBy { get; set; }
    }
}