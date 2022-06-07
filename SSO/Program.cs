using Common.Cache;
using SSO.Models;
using SSO.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IJWTService, JWTHSService>(); //对称加密
//builder.Services.AddSingleton<IJWTService, JWTRSService>(); //非对称加密
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.Configure<AppSettingsOptions>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
