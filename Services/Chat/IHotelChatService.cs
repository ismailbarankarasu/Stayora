using Stayora.Models.Chat;

namespace Stayora.Services.Chat
{
    public interface IHotelChatService
    {
        Task<ChatMessageResponse> SendMessageAsync(string conversationId, string message, CancellationToken cancellationToken = default);
    }
}