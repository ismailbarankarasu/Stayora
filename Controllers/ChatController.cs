using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stayora.Models.Chat;
using Stayora.Services.Chat;
using System.Net;
using System.Text.Json;

namespace Stayora.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ChatController : Controller
    {
        private const string ConversationKey = "HotelChatConversationId";

        private readonly IHotelChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IHotelChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            await HttpContext.Session.LoadAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString(ConversationKey)))
            {
                HttpContext.Session.SetString(
                    ConversationKey,
                    Guid.NewGuid().ToString("N"));
            }

            await HttpContext.Session.CommitAsync(cancellationToken);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("hotel-chat")]
        [RequestSizeLimit(16_384)]
        public async Task<IActionResult> Send([FromForm] ChatMessageRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    message = "Lütfen 1–2000 karakterlik bir mesaj yazın."
                });
            }

            await HttpContext.Session.LoadAsync(cancellationToken);

            var conversationId = HttpContext.Session.GetString(ConversationKey);

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return StatusCode(409, new
                {
                    message =
                        "Sohbet oturumu sona erdi. " +
                        "Sayfayı yenileyerek tekrar başlayın."
                });
            }

            try
            {
                var result = await _chatService.SendMessageAsync(
                    conversationId,
                    request.Message,
                    cancellationToken);

                return Json(result);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return StatusCode(504, new
                {
                    message =
                        "Yanıt hazırlanırken süre doldu. " +
                        "Lütfen tekrar deneyin."
                });
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Sohbet sırasında dış servis isteği başarısız oldu.");

                return StatusCode(503, new
                {
                    message = exception.StatusCode ==
                              HttpStatusCode.TooManyRequests
                        ? "AI veya otel servisinin kullanım limitine ulaşıldı. " +
                          "Lütfen daha sonra tekrar deneyin."
                        : "AI veya otel servisine şu anda ulaşılamıyor. " +
                          "Lütfen tekrar deneyin."
                });
            }
            catch (Exception exception)
                when (exception is JsonException
                    or InvalidOperationException
                    or ArgumentException)
            {
                _logger.LogError(
                    exception,
                    "Sohbet yanıtı hazırlanamadı.");

                return StatusCode(500, new
                {
                    message =
                        "Yanıt hazırlanamadı. " +
                        "Lütfen tekrar deneyin."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(CancellationToken cancellationToken)
        {
            await HttpContext.Session.LoadAsync(cancellationToken);

            HttpContext.Session.SetString(
                ConversationKey,
                Guid.NewGuid().ToString("N"));

            await HttpContext.Session.CommitAsync(cancellationToken);

            return Json(new
            {
                message = "Yeni sohbet başlatıldı."
            });
        }
        [HttpGet]
        public async Task<IActionResult> Bootstrap([FromServices] IAntiforgery antiforgery, CancellationToken cancellationToken)
        {
            await HttpContext.Session.LoadAsync(cancellationToken);

            var conversationId = HttpContext.Session.GetString(ConversationKey);

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                conversationId = Guid.NewGuid().ToString("N");

                HttpContext.Session.SetString(
                    ConversationKey,
                    conversationId);
            }

            await HttpContext.Session.CommitAsync(cancellationToken);

            var tokens = antiforgery.GetAndStoreTokens(HttpContext);

            return Json(new
            {
                requestToken = tokens.RequestToken,
                conversationId
            });
        }
    }
}