using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;
using Volunteer_website.ViewModels;
using Volunteer_website.Services;
using Volunteer_website.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register DbContext with SQL Server
builder.Services.AddDbContext<VolunteerManagementContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("VolunteerDB"));
});

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Register IHttpClientFactory (fixes the InvalidOperationException for BlogController)
builder.Services.AddHttpClient(); // This enables dependency injection for IHttpClientFactory

// Configure Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
//.AddCookie("Admin", options =>
//{
//    options.LoginPath = "/Account/Login";
//    options.LogoutPath = "/Admin/HomeAdmin/Logout";
//    options.AccessDeniedPath = "/Account/AccessDenied";
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//    options.SlidingExpiration = true;
//});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Volunteer", policy => policy.RequireRole("0"));
    options.AddPolicy("Org", policy => policy.RequireRole("1"));
    options.AddPolicy("Admin", policy => policy.RequireRole("2"));
});

// Configure API Behavior
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Configure Localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-US") };
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register VnPay Service
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Add logging for debugging (optional but recommended)
builder.Services.AddLogging(logging => logging.AddConsole());

var app = builder.Build();

// Configure Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files from wwwroot
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Configure Request Localization
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Define Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Debugging Route Info
app.Use(async (context, next) =>
{
    var routeValues = context.GetRouteData();
    Console.WriteLine($"Route Debug: {string.Join(", ", routeValues.Values.Select(kv => $"{kv.Key}={kv.Value}"))}");
    await next();
});

app.Run();