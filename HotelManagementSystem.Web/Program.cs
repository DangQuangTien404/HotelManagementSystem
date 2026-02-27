using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
builder.Services.AddDbContext<HotelManagementDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        // Fix for infinite redirect loop:
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

builder.Services.AddRazorPages();

// 3. Đăng ký các Business Services
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<CheckOutService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<MaintenanceService>();

var app = builder.Build();

// --- BẮT ĐẦU PHẦN TỰ ĐỘNG DỌN DẸP VÀ SEED DATA ---

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<HotelManagementSystem.Data.Context.HotelManagementDbContext>();
    var config = services.GetRequiredService<IConfiguration>();

    // Ensure DB Created
    context.Database.EnsureCreated();

    // ... (Phần A: Xử lý tài khoản trùng giữ nguyên) ...

    // B. TẠO TÀI KHOẢN ADMIN MẪU
    if (!context.Users.Any(u => u.Username == "admin"))
    {
        // Use configuration for sensitive data or default to safe dev values
        var adminPass = config["Seed:AdminPassword"] ?? "admin_password_placeholder"; // Changed from explicit "admin123"
        context.Users.Add(new HotelManagementSystem.Data.Models.User
        {
            Username = "admin",
            PasswordHash = adminPass,
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
        var staffPass = config["Seed:StaffPassword"] ?? "staff_password_placeholder"; // Changed from explicit "123"
        // Nếu chưa có user 'a', tạo mới luôn và nhớ thêm Email
        userA = new HotelManagementSystem.Data.Models.User
        {
            Username = "a",
            PasswordHash = staffPass,
            FullName = "Nhân viên A",
            Role = "Staff",
            Email = "staff_a@luxuryhotel.com" // THÊM DÒNG NÀY
        };
        context.Users.Add(userA);
        context.SaveChanges();
    }

    // Ensure rooms exist for testing if none
    if (!context.Rooms.Any())
    {
        context.Rooms.Add(new HotelManagementSystem.Data.Models.Room { RoomNumber = "101", RoomType = "Standard", BasePrice = 500000, Price = 500000, Status = "Available" });
        context.Rooms.Add(new HotelManagementSystem.Data.Models.Room { RoomNumber = "102", RoomType = "Deluxe", BasePrice = 800000, Price = 800000, Status = "Available" });
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

app.Run();