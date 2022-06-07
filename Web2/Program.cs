using Common;
using Common.Cache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RSAExtensions;
using System.Security.Cryptography;
using System.Text;
using Web2.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.Configure<AppSettingsOptions>(builder.Configuration.GetSection("AppSettings"));

#region 对称加密 - 鉴权
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //Audience,Issuer,clientSecret的值要和sso的一致

            //JWT有一些默认的属性，就是给鉴权时就可以筛选了
            ValidateIssuer = true,//是否验证Issuer
            ValidateAudience = true,//是否验证Audience
            ValidateLifetime = true,//是否验证失效时间
            ValidateIssuerSigningKey = true,//是否验证client secret
            ValidIssuer = builder.Configuration["AppSettings:appHSSettings:issuer"],
            ValidAudience = builder.Configuration["AppSettings:appHSSettings:audience"],//Issuer，这两项和前面签发jwt的设置一致
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:appHSSettings:clientSecret"]))//client secret
        };
    });
#endregion

#region 非对称加密-鉴权

//var rsa = RSA.Create();
//// rsa.ImportPkcs8PublicKey 是一个扩展方法，来源于 RSAExtensions 包
////byte[] publickey = Convert.FromBase64String(builder.Configuration["AppSettings:appRSSettings:audience"]); //公钥，去掉begin...  end ...
////rsa.ImportPkcs8PublicKey(publickey);

//rsa.ImportXmlPublicKey(builder.Configuration["AppSettings:appRSSettings:audience"]);
//var key = new RsaSecurityKey(rsa);
//var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaPKCS1);

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            //Audience,Issuer,clientSecret的值要和sso的一致

//            //JWT有一些默认的属性，就是给鉴权时就可以筛选了
//            ValidateIssuer = true,//是否验证Issuer
//            ValidateAudience = true,//是否验证Audience
//            ValidateLifetime = true,//是否验证失效时间
//            ValidateIssuerSigningKey = true,//是否验证client secret
//            ValidIssuer = builder.Configuration["AppSettings:appRSSettings:issuer"],//
//            ValidAudience = builder.Configuration["AppSettings:appRSSettings:audience"],//Issuer，这两项和前面签发jwt的设置一致
//            IssuerSigningKey = signingCredentials.Key
//        };
//    });

#endregion

var app = builder.Build();

ServiceLocator.Instance = app.Services; //用于手动获取DI对象

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();//这个加在UseAuthorization 前
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
