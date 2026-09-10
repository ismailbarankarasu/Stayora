using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Stayora.Controllers
{
    public class GeminiDiagnosticsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;

        public GeminiDiagnosticsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
        {
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Models(CancellationToken cancellationToken)
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
        [HttpGet]
        public IActionResult Test()
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestReply([FromServices] IConfiguration configuration, CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            var model = configuration["GeminiApi:Model"];

            if (string.IsNullOrWhiteSpace(model))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "GeminiApi:Model ayarı bulunamadı."
                });
            }

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                role = "user",
                parts = new[]
                {
                    new
                    {
                        text =
                            "Türkçe tek cümleyle Stayora kullanıcısını " +
                            "selamla ve hangi şehre gitmek istediğini sor. " +
                            "Otel veya fiyat önerme."
                    }
                }
            }
        },
                generationConfig = new
                {
                    maxOutputTokens = 1024
                }
            };

            try
            {
                var client = _httpClientFactory.CreateClient("Gemini");

                using var response = await client.PostAsJsonAsync(
                    $"v1beta/models/{Uri.EscapeDataString(model)}:generateContent",
                    requestBody,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;

                    var errorBody = await response.Content.ReadAsStringAsync(
                        cancellationToken);

                    string? providerMessage = null;

                    try
                    {
                        using var errorDocument = JsonDocument.Parse(errorBody);

                        if (errorDocument.RootElement.TryGetProperty(
                                "error", out var error)
                            && error.TryGetProperty("message", out var message))
                        {
                            providerMessage = message.GetString();
                        }
                    }
                    catch (JsonException)
                    {
                        providerMessage = "Servis JSON biçiminde hata açıklaması döndürmedi.";
                    }

                    return StatusCode(statusCode, new
                    {
                        success = false,
                        statusCode,
                        model,
                        requestPath = response.RequestMessage?.RequestUri?.AbsolutePath,
                        message = providerMessage ?? "Hata açıklaması bulunamadı."
                    });
                }
                using var stream = await response.Content
                    .ReadAsStreamAsync(cancellationToken);

                using var document = await JsonDocument.ParseAsync(
                    stream,
                    cancellationToken: cancellationToken);

                var texts = new List<string>();

                if (document.RootElement.TryGetProperty(
                        "candidates", out var candidates)
                    && candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0];

                    if (candidate.TryGetProperty("content", out var content)
                        && content.TryGetProperty("parts", out var parts))
                    {
                        foreach (var part in parts.EnumerateArray())
                        {
                            if (part.TryGetProperty("thought", out var thought)
                                && thought.ValueKind == JsonValueKind.True)
                            {
                                continue;
                            }

                            if (part.TryGetProperty("text", out var text))
                            {
                                var value = text.GetString();

                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    texts.Add(value);
                                }
                            }
                        }
                    }
                }

                if (texts.Count == 0)
                {
                    return StatusCode(502, new
                    {
                        success = false,
                        message = "Gemini cevabında gösterilebilir metin bulunamadı."
                    });
                }

                return Json(new
                {
                    success = true,
                    model,
                    reply = string.Join("\n", texts)
                });
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return StatusCode(504, new
                {
                    success = false,
                    message = "Gemini yanıtı zaman aşımına uğradı."
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
            catch (JsonException)
            {
                return StatusCode(502, new
                {
                    success = false,
                    message = "Gemini cevabı okunamadı."
                });
            }
        }
    }
}