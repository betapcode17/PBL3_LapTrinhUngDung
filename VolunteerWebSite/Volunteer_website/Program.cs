using Microsoft.EntityFrameworkCore;
using Volunteer_website.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Volunteer_website.Identity;
using Volunteer_website.Models;
using Volunteer_website.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<VolunteerManagementContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("VolunteerDB"));
});



builder.Services.AddAutoMapper(typeof(AutoMapperProfile));


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
// Cấu hình dịch vụ Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
        options.LogoutPath = "/Account/Logout"; // Đường dẫn đến trang đăng xuất
        options.AccessDeniedPath = "/Account/AccessDenied"; // Trang khi không có quyền truy cập
    });

builder.Services.AddAuthorization(); // Bật Authorization
// Sử dụng Authentication và Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Định tuyến Controller

app.Run();
