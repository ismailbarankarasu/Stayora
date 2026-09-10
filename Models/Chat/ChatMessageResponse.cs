namespace Stayora.Models.Chat
{
    public class ChatMessageResponse
    {
        public string Message { get; set; } = string.Empty;

        public List<ChatHotelCard> Hotels { get; set; } = [];

        public ChatHotelSearchCriteria? SearchCriteria { get; set; }
    }
}