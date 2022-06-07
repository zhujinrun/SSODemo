using Common;
using Common.Cache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RSAExtensions;
using System.Security.Cryptography;
using System.Text;
using Web1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.Configure<AppSettingsOptions>(builder.Configuration.GetSection("AppSettings"));

#region �ԳƼ��� - ��Ȩ

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            //Audience,Issuer,clientSecret��ֵҪ��sso��һ��

//            //JWT��һЩĬ�ϵ����ԣ����Ǹ���Ȩʱ�Ϳ���ɸѡ��
//            ValidateIssuer = true,//�Ƿ���֤Issuer
//            ValidateAudience = true,//�Ƿ���֤Audience
//            ValidateLifetime = true,//�Ƿ���֤ʧЧʱ��
//            ValidateIssuerSigningKey = true,//�Ƿ���֤client secret
//            ValidIssuer = builder.Configuration["AppSettings:appHSSettings:issuer"],
//            ValidAudience = builder.Configuration["AppSettings:appHSSettings:audience"],//Issuer���������ǰ��ǩ��jwt������һ��
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:appHSSettings:clientSecret"]))//client secret
//        };
//    });

#endregion

#region �ǶԳƼ���-��Ȩ

var rsa = RSA.Create();

// rsa.ImportPkcs8PublicKey �� ImportXmlPublicKey ������չ��������Դ�� RSAExtensions ��

//byte[] publickey = Convert.FromBase64String(builder.Configuration["AppSettings:appRSSettings:publicKey"]); //��Կ��ȥ��begin...  end ...
//rsa.ImportPkcs8PublicKey(publickey);

rsa.ImportXmlPublicKey(builder.Configuration["AppSettings:appRSSettings:publicKey"]);
var key = new RsaSecurityKey(rsa);
var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaPKCS1);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //Audience,Issuer,clientSecret��ֵҪ��sso��һ��

            //JWT��һЩĬ�ϵ����ԣ����Ǹ���Ȩʱ�Ϳ���ɸѡ��
            ValidateIssuer = true,//�Ƿ���֤Issuer
            ValidateAudience = true,//�Ƿ���֤Audience
            ValidateLifetime = true,//�Ƿ���֤ʧЧʱ��
            ValidateIssuerSigningKey = true,//�Ƿ���֤client secret
            ValidIssuer = builder.Configuration["AppSettings:appRSSettings:issuer"],//
            ValidAudience = builder.Configuration["AppSettings:appRSSettings:audience"],//Issuer���������ǰ��ǩ��jwt������һ��
            IssuerSigningKey = signingCredentials.Key
        };
    });

#endregion

var app = builder.Build();

ServiceLocator.Instance = app.Services; //�����ֶ���ȡDI����

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();//�������UseAuthorization ǰ
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
