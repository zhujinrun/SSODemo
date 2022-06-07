using Common.Cache;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RSAExtensions;
using SSO.Models;
using System.Security.Cryptography;

namespace SSO.Services
{
    /// <summary>
    /// JWT非对称加密
    /// </summary>
    public class JWTRSService : JWTBaseService
    {
        private readonly IOptions<AppSettingsOptions> _options;
        private readonly ICacheService _cache;

        public JWTRSService(IOptions<AppSettingsOptions> options, ICacheService cache)
            : base(options, cache)
        {
            _options = options;
            _cache = cache;
        }

        /// <summary>
        /// 生成非对称加密签名凭证
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        protected override SigningCredentials GetCredentials(string clientId)
        {
            var appRSSetting = GetAppSetting(clientId);

            var rsa = RSA.Create();

            // rsa.ImportPkcs8PrivateKey 是一个扩展方法，来源于 RSAExtensions 包
            //byte[] privateKey = Convert.FromBase64String(appRSSetting.PrivateKey);//这里只需要私钥，不要begin,不要end
            //rsa.ImportPkcs8PrivateKey(privateKey, out _);

            rsa.ImportXmlPrivateKey(appRSSetting.PrivateKey);

            var key = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            return credentials;
        }

        /// <summary>
        /// 根据appKey获取应用信息
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private AppRSSetting GetAppSetting(string clientId)
        {
            AppRSSetting appRSSetting = _options.Value.AppRSSettings.Where(s => s.ClientId == clientId).FirstOrDefault();
            return appRSSetting;
        }
    }
}
