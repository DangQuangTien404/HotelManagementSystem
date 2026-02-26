using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HotelManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/AccessDenied";
    });

// 2. Đăng ký AuthService

builder.Services.AddRazorPages();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<CheckOutService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<DataCleanupService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var cleanupService = scope.ServiceProvider.GetRequiredService<DataCleanupService>();
    await cleanupService.CleanDuplicateUsersAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ai là người đang truy cập?

app.UseAuthorization();

app.MapRazorPages();

app.Run();
