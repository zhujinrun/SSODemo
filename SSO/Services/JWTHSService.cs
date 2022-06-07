using Common.Cache;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SSO.Models;
using System.Text;

namespace SSO.Services
{
    /// <summary>
    /// JWT对称可逆加密
    /// </summary>
    public class JWTHSService : JWTBaseService
    {
        private readonly IOptions<AppSettingsOptions> _options;
        private readonly ICacheService _cache;

        public JWTHSService(IOptions<AppSettingsOptions> options, ICacheService cache)
            : base(options, cache)
        {
            _options = options;
            _cache = cache;
        }

        /// <summary>
        /// 生成对称加密签名凭证
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        protected override SigningCredentials GetCredentials(string clientId)
        {
            var appHSSettings = GetAppSetting(clientId);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appHSSettings.ClientSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            return credentials;
        }

        /// <summary>
        /// 根据appKey获取应用信息
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private AppHSSetting GetAppSetting(string clientId)
        {
            AppHSSetting appHSSetting = _options.Value.AppHSSettings.Where(s => s.ClientId == clientId).FirstOrDefault();
            return appHSSetting;
        }
    }
}
