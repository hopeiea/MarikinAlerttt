using MarikinAlert.Web.Services;
using MarikinAlert.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// =================================================================
// 1. DATABASE CONNECTION (SQLite - Presentation Safe)
// =================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=marikina.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())); 

// =================================================================
// 2. REPOSITORY SERVICES
// =================================================================
builder.Services.AddScoped<IDisasterRepository, DisasterRepository>();

// =================================================================
// 3. CACHING & SESSION
// =================================================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// =================================================================
// 4. AUTHENTICATION
// =================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// =================================================================
// 5. APPLICATION SERVICES
// =================================================================
builder.Services.AddSingleton<TaglishTextScanner>();
builder.Services.AddScoped<ITextScanner>(provider =>
{
    var fallback = provider.GetRequiredService<TaglishTextScanner>();
    return new MLTextScanner(fallback); 
});

builder.Services.AddSingleton<IDisasterTriageService, AiTriageService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// =================================================================
// MIDDLEWARE PIPELINE (Order matters!)
// =================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reports}/{action=Create}/{id?}");

// =================================================================
// SEED DATA
// =================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(context);
}

app.Run();