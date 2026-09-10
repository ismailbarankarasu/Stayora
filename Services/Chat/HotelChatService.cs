using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using Stayora.Models;
using Stayora.Models.Chat;

namespace Stayora.Services.Chat
{
    public class HotelChatService : IHotelChatService
    {
        private readonly GeminiChatClient _gemini;
        private readonly IBookingService _bookingService;
        private readonly IMemoryCache _cache;
        private readonly LinkGenerator _links;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly object StateCreationLock = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class ConversationState
        {
            public SemaphoreSlim Gate { get; } = new(1, 1);

            public JsonArray History { get; set; } = new();

            public int TurnCount { get; set; }
        }

        public HotelChatService(GeminiChatClient gemini, IBookingService bookingService, IMemoryCache cache, LinkGenerator links, IHttpContextAccessor httpContextAccessor)
        {
            _gemini = gemini;
            _bookingService = bookingService;
            _cache = cache;
            _links = links;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ChatMessageResponse> SendMessageAsync(string conversationId, string message, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(conversationId, out var conversationGuid))
            {
                throw new ArgumentException("Geçersiz konuşma kimliği.");
            }

            if (string.IsNullOrWhiteSpace(message) || message.Length > 2000)
            {
                throw new ArgumentException(
                    "Mesaj 1 ile 2000 karakter arasında olmalıdır.");
            }

            var cacheKey = $"hotel-chat:{conversationGuid:N}";

            ConversationState state;

            lock (StateCreationLock)
            {
                if (!_cache.TryGetValue(
                        cacheKey,
                        out ConversationState? existing)
                    || existing is null)
                {
                    existing = new ConversationState();

                    _cache.Set(
                        cacheKey,
                        existing,
                        new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(30)
                        });
                }

                state = existing;
            }

            await state.Gate.WaitAsync(cancellationToken);

            try
            {
                if (state.TurnCount >= 20)
                {
                    return new ChatMessageResponse
                    {
                        Message =
                            "Bu sohbetin mesaj sınırına ulaştık. " +
                            "Yeni sohbet başlatarak devam edebilirsiniz."
                    };
                }

                // İşlem başarısız olursa mevcut geçmiş bozulmasın.
                var history = (JsonArray)state.History.DeepClone();

                history.Add(CreateTextContent("user", message.Trim()));

                var modelContent = await _gemini.GenerateTurnAsync(
                    history,
                    allowSearch: true,
                    cancellationToken: cancellationToken);

                var calls = (modelContent["parts"] as JsonArray)?
                    .OfType<JsonObject>()
                    .Where(part => part["functionCall"] is JsonObject)
                    .Select(part => part["functionCall"]!.AsObject())
                    .ToList() ?? new List<JsonObject>();

                if (calls.Count > 1)
                {
                    throw new InvalidOperationException(
                        "Bir mesajda birden fazla arama isteği üretildi.");
                }

                history.Add(modelContent.DeepClone());

                var reply = new ChatMessageResponse();

                if (calls.Count == 0)
                {
                    reply.Message = ReadText(modelContent);
                }
                else
                {
                    var call = calls[0];

                    if (call["name"]?.GetValue<string>() != "search_hotels")
                    {
                        throw new InvalidOperationException(
                            "Desteklenmeyen araç çağrısı.");
                    }

                    var toolResult = await ExecuteSearchAsync(
                        call["args"] as JsonObject,
                        reply,
                        cancellationToken);

                    var functionResponse = new JsonObject
                    {
                        ["name"] = "search_hotels",
                        ["response"] = toolResult
                    };

                    // API çağrı kimliği döndürdüyse aynen koru.
                    if (call["id"] is JsonNode callId)
                    {
                        functionResponse["id"] = callId.DeepClone();
                    }

                    history.Add(new JsonObject
                    {
                        ["role"] = "user",
                        ["parts"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["functionResponse"] = functionResponse
                            }
                        }
                    });

                    var finalContent = await _gemini.GenerateTurnAsync(
                        history,
                        allowSearch: false,
                        cancellationToken: cancellationToken);

                    if ((finalContent["parts"] as JsonArray)?
                        .OfType<JsonObject>()
                        .Any(part => part["functionCall"] is not null) == true)
                    {
                        throw new InvalidOperationException(
                            "Beklenmeyen ek araç çağrısı.");
                    }

                    reply.Message = ReadText(finalContent);
                    history.Add(finalContent.DeepClone());
                }

                if (string.IsNullOrWhiteSpace(reply.Message))
                {
                    throw new InvalidOperationException(
                        "Asistan gösterilebilir bir yanıt üretmedi.");
                }

                state.History = history;
                state.TurnCount++;

                return reply;
            }
            finally
            {
                state.Gate.Release();
            }
        }

        private async Task<JsonObject> ExecuteSearchAsync(JsonObject? arguments, ChatMessageResponse reply, CancellationToken cancellationToken)
        {
            ChatHotelSearchCriteria? criteria;

            try
            {
                criteria = arguments?.Deserialize<ChatHotelSearchCriteria>(JsonOptions);
            }
            catch (JsonException)
            {
                return ToolError(
                    "Arama bilgileri okunamadı. Tarihleri ve sayıları netleştir.");
            }

            if (criteria is null ||
                string.IsNullOrWhiteSpace(criteria.City) ||
                !criteria.CheckIn.HasValue ||
                !criteria.CheckOut.HasValue ||
                !criteria.Adults.HasValue ||
                !criteria.Rooms.HasValue)
            {
                return ToolError(
                    "Şehir, giriş, çıkış, yetişkin ve oda bilgileri zorunlu. " +
                    "Eksik bilgileri kullanıcıya sor.");
            }

            var request = new HotelSearchRequest
            {
                City = criteria.City.Trim(),
                CheckIn = criteria.CheckIn,
                CheckOut = criteria.CheckOut,
                Adults = criteria.Adults.Value,
                Rooms = criteria.Rooms.Value,
                MinPrice = criteria.MinPrice,
                MaxPrice = criteria.MaxPrice,
                CategoryFilter = criteria.CategoryFilter,
                SortBy = string.IsNullOrWhiteSpace(criteria.SortBy)
                    ? "popularity"
                    : criteria.SortBy,
                PageNumber = 1
            };

            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(
                    request,
                    new ValidationContext(request),
                    validationResults,
                    validateAllProperties: true))
            {
                return ToolError(string.Join(
                    " ",
                    validationResults.Select(x => x.ErrorMessage)));
            }

            reply.SearchCriteria = criteria;

            var destinations =
                await _bookingService.SearchDestinationsAsync(
                    request.City,
                    cancellationToken);

            var destination = destinations
                .Take(2)
                .FirstOrDefault(x =>
                    string.Equals(
                        x.SearchType,
                        "city",
                        StringComparison.OrdinalIgnoreCase));

            if (destination is null)
            {
                return ToolError(
                    "Şehir bulunamadı. Kullanıcıdan şehir adını " +
                    "ve gerekirse ülkesini netleştirmesini iste.");
            }

            var result = await _bookingService.SearchHotelsAsync(
                destination,
                request,
                cancellationToken);

            var context = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException(
                    "HTTP bağlamı bulunamadı.");

            foreach (var item in result.Hotels)
            {
                var property = item.Property;

                if (property is null || item.HotelId <= 0)
                {
                    continue;
                }

                var detailsUrl = _links.GetPathByAction(
                    context,
                    action: "Details",
                    controller: "Hotels",
                    values: new
                    {
                        hotelId = item.HotelId,
                        City = request.City,

                        CheckIn = request.CheckIn.Value.ToString(
                            "yyyy-MM-dd", CultureInfo.InvariantCulture),

                        CheckOut = request.CheckOut.Value.ToString(
                            "yyyy-MM-dd", CultureInfo.InvariantCulture),

                        request.Adults,
                        request.Rooms,
                        request.SortBy,
                        request.CategoryFilter,

                        MinPrice = request.MinPrice?.ToString(
                            CultureInfo.InvariantCulture),

                        MaxPrice = request.MaxPrice?.ToString(
                            CultureInfo.InvariantCulture),

                        PageNumber = 1
                    });

                if (string.IsNullOrWhiteSpace(detailsUrl))
                {
                    throw new InvalidOperationException(
                        "Otel detay bağlantısı oluşturulamadı.");
                }

                var price = property.PriceBreakdown?.GrossPrice;
                var excluded = property.PriceBreakdown?.ExcludedPrice;

                reply.Hotels.Add(new ChatHotelCard
                {
                    HotelId = item.HotelId,
                    Name = property.Name,
                    PhotoUrl = property.PhotoUrls.FirstOrDefault(),
                    ReviewScore = property.ReviewScore,
                    ReviewCount = property.ReviewCount,
                    Price = price?.Value,
                    Currency = price?.Currency,
                    ExcludedPrice = excluded?.Value,
                    ExcludedPriceCurrency = excluded?.Currency,

                    PriceConditions = property.PriceBreakdown?.BenefitBadges
                        .Select(x => x.Text)
                        .OfType<string>()
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList() ?? new List<string>(),

                    DetailsUrl = detailsUrl
                });
            }

            return JsonSerializer.SerializeToNode(new
            {
                success = true,
                city = destination.Name,
                search = request,
                displayedCount = reply.Hotels.Count,

                hotels = reply.Hotels.Select(hotel => new
                {
                    hotel.Name,
                    hotel.ReviewScore,
                    hotel.Price,
                    hotel.Currency,
                    hotel.ExcludedPrice,
                    hotel.ExcludedPriceCurrency,
                    hotel.PriceConditions
                }),

                instruction = reply.Hotels.Count == 0
                    ? "Bu aramada sonuç bulunamadı. Kriterleri değiştirmeyi öner."
                    : "Bunlar gösterilen ilk seçeneklerdir. Kısa özet ver. " +
                      "Kartları incelemeye yönlendir. URL oluşturma. " +
                      "Vergiler veya bütçe uyumu hakkında kesinlik uydurma."
            })!.AsObject();
        }

        private static JsonObject ToolError(string message)
        {
            return new JsonObject
            {
                ["success"] = false,
                ["message"] = message
            };
        }

        private static JsonObject CreateTextContent(string role, string text)
        {
            return new JsonObject
            {
                ["role"] = role,
                ["parts"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["text"] = text
                    }
                }
            };
        }

        private static string ReadText(JsonObject content)
        {
            var parts = content["parts"] as JsonArray;

            if (parts is null)
            {
                return string.Empty;
            }

            var texts = parts
                .OfType<JsonObject>()
                .Where(part =>
                    part["thought"]?.GetValue<bool>() != true &&
                    part["text"] is JsonValue)
                .Select(part => part["text"]!.GetValue<string>())
                .Where(text => !string.IsNullOrWhiteSpace(text));

            return string.Join("\n", texts);
        }
    }
}