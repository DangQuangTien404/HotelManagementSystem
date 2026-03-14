using HotelManagementSystem.Business;
using HotelManagementSystem.Business.service;
using HotelManagementSystem.Business.interfaces;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
DotNetEnv.Env.Load();
builder.Services.AddDbContext<HotelManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRoomUpdateBroadcaster, HotelManagementSystem.Web.Services.RoomUpdateBroadcaster>();

// 3. Đăng ký các Business Services
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
var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
var openAiModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";

builder.Services.AddKernel()
    .AddOpenAIChatCompletion(openAiModel, openAiKey);

builder.Services.AddScoped<IChatbotService, ChatbotService>();
// -------------------------

builder.Services.AddHostedService<NoShowSweepService>();

var app = builder.Build();

// --- BẮT ĐẦU PHẦN TỰ ĐỘNG DỌN DẸP VÀ SEED DATA ---

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<HotelManagementSystem.Data.Context.HotelManagementDbContext>();

    // ... (Phần A: Xử lý tài khoản trùng giữ nguyên) ...

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
// --- KẾT THÚC PHẦN SEED DATA ---

// Configure the HTTP request pipeline.
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

app.MapPost("/api/momo-ipn", async (HttpContext context, IBookingService bookingService, IMoMoService momoService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var data = System.Text.Json.JsonSerializer.Deserialize<MoMoCallbackData>(
        body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (data == null) return Results.BadRequest();

    if (!momoService.VerifySignature(data)) return Results.BadRequest();

    if (data.ResultCode == 0)
        await bookingService.ConfirmPaymentAsync(data.OrderId, data.TransId.ToString());
    else
        await bookingService.FailPaymentAsync(data.OrderId);

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