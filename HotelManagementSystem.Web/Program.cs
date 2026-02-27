using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext
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

// 3. Đăng ký các Business Services
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<CheckOutService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();

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
        context.Users.Add(new HotelManagementSystem.Models.User
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
        userA = new HotelManagementSystem.Models.User
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
        context.Staffs.Add(new HotelManagementSystem.Models.Staff
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