using DotNetEnv;
using HotelManagementSystem.Business;
using HotelManagementSystem.Business.interfaces;
using HotelManagementSystem.Business.service;
using HotelManagementSystem.Data.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

Env.Load();
Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
DotNetEnv.Env.Load();
builder.Services.AddDbContext<HotelManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRoomUpdateBroadcaster, HotelManagementSystem.Web.Services.RoomUpdateBroadcaster>();
builder.Services.AddScoped<IReservationUpdateBroadcaster, HotelManagementSystem.Web.Services.ReservationUpdateBroadcaster>();

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ICheckOutService, CheckOutService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ICleaningService, CleaningService>();
builder.Services.AddHttpClient<IMoMoService, MoMoService>();

// --- CONFIG CHATBOT AI ---
var openAiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
var openAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";

// Plugin và Service đều Scoped
builder.Services.AddScoped<HotelManagementSystem.Business.service.HotelDataPlugin>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

// Kernel là Scoped — tạo mới theo từng request, inject được DbContext
builder.Services.AddScoped<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.AddOpenAIChatCompletion(openAiModel, openAiKey);

    // Lấy plugin từ DI scope hiện tại — DbContext cùng scope → OK
    kernelBuilder.Plugins.AddFromObject(
        sp.GetRequiredService<HotelManagementSystem.Business.service.HotelDataPlugin>(),
        pluginName: "HotelPlugin"
    );

    return kernelBuilder.Build();
});
// -------------------------

builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddHttpClient<IStripeService, StripeService>();
builder.Services.AddHostedService<NoShowSweepService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<HotelManagementDbContext>();

    // B. TẠO TÀI KHOẢN ADMIN MẪU
    if (!context.Users.Any(u => u.Username == "admin"))
    {
        context.Users.Add(new HotelManagementSystem.Data.Models.User
        {
            Username = "admin",
            PasswordHash = "admin123",
            FullName = "Quản trị viên",
            Role = "Admin",
            Email = "admin@luxuryhotel.com" // THÊM DÒNG NÀY (Hoặc email bất kỳ)
        });
        context.SaveChanges();
    }

    // C. KIỂM TRA TÀI KHOẢN 'a' VÀ CẤP QUYỀN STAFF
    var userA = context.Users.FirstOrDefault(u => u.Username == "a");
    if (userA == null)
    {
        // Nếu chưa có user 'a', tạo mới luôn và nhớ thêm Email
        userA = new HotelManagementSystem.Data.Models.User
        {
            Username = "a",
            PasswordHash = "123",
            FullName = "Nhân viên A",
            Role = "Staff",
            Email = "staff_a@luxuryhotel.com" // THÊM DÒNG NÀY
        };
        context.Users.Add(userA);
        context.SaveChanges();
    }

    // Đảm bảo user 'a' có trong bảng Staff
    var isStaffExist = context.Staffs.Any(s => s.UserId == userA.Id);
    if (!isStaffExist)
    {
        context.Staffs.Add(new HotelManagementSystem.Data.Models.Staff
        {
            UserId = userA.Id,
            Position = "Dọn dẹp",
            Shift = "Ca làm việc mặc định"
        });
        context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<HotelManagementSystem.Web.Hubs.NotificationHub>("/notificationHub");
app.MapHub<HotelManagementSystem.Web.Hubs.RoomHub>("/roomHub");
app.MapHub<HotelManagementSystem.Web.Hubs.ChatHub>("/chatHub");
app.MapHub<HotelManagementSystem.Web.Hubs.ReservationHub>("/reservationHub");

// Stripe webhook
app.MapPost("/api/stripe-webhook", async (HttpContext context, IBookingService bookingService, IStripeService stripeService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var payload = await reader.ReadToEndAsync();
    var signatureHeader = context.Request.Headers["Stripe-Signature"].ToString();

    var stripeEvent = stripeService.ParseWebhookEvent(payload, signatureHeader);
    if (stripeEvent == null)
    {
        return Results.BadRequest();
    }

    if (string.IsNullOrWhiteSpace(stripeEvent.OrderId))
    {
        return Results.NoContent();
    }

    if ((string.Equals(stripeEvent.Type, "checkout.session.completed", StringComparison.OrdinalIgnoreCase)
         || string.Equals(stripeEvent.Type, "checkout.session.async_payment_succeeded", StringComparison.OrdinalIgnoreCase))
        && string.Equals(stripeEvent.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
    {
        var transactionId = stripeEvent.TransactionId ?? stripeEvent.SessionId ?? stripeEvent.OrderId;
        await bookingService.ConfirmPaymentAsync(stripeEvent.OrderId, transactionId);
    }
    else if (string.Equals(stripeEvent.Type, "checkout.session.expired", StringComparison.OrdinalIgnoreCase)
             || string.Equals(stripeEvent.Type, "checkout.session.async_payment_failed", StringComparison.OrdinalIgnoreCase))
    {
        await bookingService.FailPaymentAsync(stripeEvent.OrderId);
    }

    return Results.NoContent();
});

app.MapPost("/api/chat-ai", async (HttpContext context, IChatbotService chatbotService) =>
{
    var data = await context.Request.ReadFromJsonAsync<ChatRequest>();
    if (data == null || string.IsNullOrEmpty(data.Message)) return Results.BadRequest();

    var response = await chatbotService.GetChatResponseAsync(data.Message);
    return Results.Ok(new { response });
});

app.Run();

public record ChatRequest(string Message);