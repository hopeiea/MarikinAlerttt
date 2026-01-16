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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();