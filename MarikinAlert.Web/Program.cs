using MarikinAlert.Web.Services;
using MarikinAlert.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// =================================================================
// 1. DATABASE CONNECTION (SQLite - Presentation Safe)
// =================================================================

// This looks for "Data Source=marikina.db" in your appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=marikina.db"; // Fallback if appsettings is missing

// CHANGED: UseSqlite instead of UseSqlServer
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)); 

// =================================================================

builder.Services.AddScoped<IDisasterRepository, DisasterRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddSingleton<TaglishTextScanner>();
builder.Services.AddScoped<ITextScanner>(provider =>
{
    var fallback = provider.GetRequiredService<TaglishTextScanner>();
    return new MLTextScanner(fallback); 
});

builder.Services.AddScoped<IDisasterTriageService, TriageService>();

builder.Services.AddHttpContextAccessor();

// 1. Add Memory Cache (Session requires a place to store data)
builder.Services.AddDistributedMemoryCache();

// 2. Add Session Service
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Log out after 30 mins inactive
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ---> ADD THIS LINE HERE <---
app.UseSession();
// ----------------------------

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reports}/{action=Create}/{id?}");

app.Run();