using Stayora.Services;
using Stayora.Services.Chat;
using System.Threading.RateLimiting;

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
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".Stayora.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("hotel-chat", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Session.Id,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 6,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                message =
                    "Kısa sürede çok fazla mesaj gönderdiniz. " +
                    "Lütfen bir dakika sonra tekrar deneyin."
            },
            cancellationToken);
    };
});

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
app.UseSession();
app.UseRateLimiter();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
