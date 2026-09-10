using Stayora.Services;
using Stayora.Services.Chat;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<IBookingService, BookingService>(
    (serviceProvider, client) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiKey = configuration["BookingApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "BookingApi:ApiKey ayarı bulunamadı.");
        }

        client.BaseAddress = new Uri(
            "https://booking-com15.p.rapidapi.com/");

        client.Timeout = TimeSpan.FromSeconds(30);

        client.DefaultRequestHeaders.Add(
            "x-rapidapi-key", apiKey);

        client.DefaultRequestHeaders.Add(
            "x-rapidapi-host",
            "booking-com15.p.rapidapi.com");
    });
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient(
    "Gemini",
    (serviceProvider, client) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiKey = configuration["GeminiApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "GeminiApi:ApiKey ayarı bulunamadı.");
        }

        client.BaseAddress = new Uri(
            "https://generativelanguage.googleapis.com/");

        client.Timeout = TimeSpan.FromSeconds(60);

        client.DefaultRequestHeaders.Add(
            "x-goog-api-key",
            apiKey);
    });

builder.Services.AddScoped<GeminiChatClient>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IHotelChatService, HotelChatService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
