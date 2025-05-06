using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;
using Volunteer_website.ViewModels;
using Volunteer_website.Services;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian session tồn tại
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//Connect Vnpay
builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddDbContext<VolunteerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase"));
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
app.UseStaticFiles(); // Đảm bảo phục vụ các tệp tĩnh từ wwwroot

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
