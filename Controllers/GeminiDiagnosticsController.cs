using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Stayora.Controllers
{
    public class GeminiDiagnosticsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;

        public GeminiDiagnosticsController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Models(
            CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("Gemini");

                using var response = await client.GetAsync(
                    "v1beta/models?pageSize=1000",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            success = false,
                            statusCode = (int)response.StatusCode,
                            message =
                                "Gemini model listesi alınamadı. " +
                                "Anahtar, API erişimi ve proje ayarlarını kontrol edin."
                        });
                }

                using var stream = await response.Content
                    .ReadAsStreamAsync(cancellationToken);

                using var document = await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: cancellationToken);

                var models = new List<string>();

                if (document.RootElement.TryGetProperty(
                    "models", out var modelArray))
                {
                    foreach (var model in modelArray.EnumerateArray())
                    {
                        if (!model.TryGetProperty(
                            "supportedGenerationMethods",
                            out var methods))
                        {
                            continue;
                        }

                        var supportsGeneration = methods
                            .EnumerateArray()
                            .Any(method =>
                                method.GetString() == "generateContent");

                        if (supportsGeneration &&
                            model.TryGetProperty("name", out var name))
                        {
                            var modelName = name.GetString();

                            if (!string.IsNullOrWhiteSpace(modelName))
                            {
                                models.Add(modelName);
                            }
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Gemini bağlantısı başarılı.",
                    models,
                    hasMore = document.RootElement.TryGetProperty(
                        "nextPageToken", out _)
                });
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return StatusCode(504, new
                {
                    success = false,
                    message = "Gemini bağlantısı zaman aşımına uğradı."
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    success = false,
                    message = "Gemini servisine bağlantı kurulamadı."
                });
            }
        }
    }
}