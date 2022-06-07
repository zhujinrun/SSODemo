using Common;
using Common.Cache;
using Common.Dto;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SSO.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SSO.Services
{
    /// <summary>
    /// jwt服务
    /// </summary>
    public abstract class JWTBaseService : IJWTService
    {
        private readonly IOptions<AppSettingsOptions> _options;
        private readonly ICacheService _cache;

        public JWTBaseService(IOptions<AppSettingsOptions> options, ICacheService cache)
        {
            _options = options;
            _cache = cache;
        }

        /// <summary>
        /// 获取授权码
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ServerResponse<string> GetCode(string clientId, string userName, string password)
        {
            ServerResponse<string> result = new ServerResponse<string>();

            string code = string.Empty;
            AppHSSetting appHSSetting = _options.Value.AppHSSettings.Where(s => s.ClientId == clientId).FirstOrDefault();
            if (appHSSetting == null)
            {
                result.SetFail("应用不存在");
                return result;
            }
            //真正项目这里查询数据库比较
            if (!(userName == "admin" && password == "123456"))
            {
                result.SetFail("用户名或密码不正确");
                return result;
            }

            //用户信息
            var currentUserModel = new CurrentUserModel
            {
                Id = 101,
                Account = "admin",
                Name = "张三",
                Mobile = "13800138000",
                Role = "SuperAdmin"
            };

            //生成授权码
            code = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            string key = $"AuthCode:{code}";
            string appCachekey = $"AuthCodeClientId:{code}";
            //缓存授权码
            _cache.StringSet<CurrentUserModel>(key, currentUserModel, TimeSpan.FromMinutes(10));
            //缓存授权码是哪个应用的
            _cache.StringSet<string>(appCachekey, appHSSetting.ClientId, TimeSpan.FromMinutes(10));
            //创建全局会话
            string sessionCode = $"SessionCode:{code}";
            var sessionCodeUser = new SessionCodeUser
            {
                ExpiresTime = DateTime.Now.AddHours(1),
                CurrentUser = currentUserModel
            };
            _cache.StringSet<CurrentUserModel>(sessionCode, currentUserModel, TimeSpan.FromDays(1));
            //全局会话过期时间
            string sessionExpiryKey = $"SessionExpiryKey:{code}";
            DateTime sessionExpirTime = DateTime.Now.AddDays(1);
            _cache.StringSet<DateTime>(sessionExpiryKey, sessionExpirTime, TimeSpan.FromDays(1));
            Console.WriteLine($"登录成功，全局会话code:{code}");
            //缓存授权码取token时最长的有效时间
            _cache.StringSet<DateTime>($"AuthCodeSessionTime:{code}", sessionExpirTime, TimeSpan.FromDays(1));

            result.SetSuccess(code);
            return result;
        }

        /// <summary>
        /// 根据会话code获取授权码
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="sessionCode"></param>
        /// <returns></returns>
        public ServerResponse<string> GetCodeBySessionCode(string clientId, string sessionCode)
        {
            ServerResponse<string> result = new ServerResponse<string>();
            string code = string.Empty;
            AppHSSetting appHSSetting = _options.Value.AppHSSettings.Where(s => s.ClientId == clientId).FirstOrDefault();
            if (appHSSetting == null)
            {
                result.SetFail("应用不存在");
                return result;
            }
            string codeKey = $"SessionCode:{sessionCode}";
            var currentUserModel = _cache.StringGet<CurrentUserModel>(codeKey);
            if (currentUserModel == null)
            {
                return result.SetFail("会话不存在或已过期", string.Empty);
            }

            //生成授权码
            code = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            string key = $"AuthCode:{code}";
            string appCachekey = $"AuthCodeClientId:{code}";
            //缓存授权码
            _cache.StringSet<CurrentUserModel>(key, currentUserModel, TimeSpan.FromMinutes(10));
            //缓存授权码是哪个应用的
            _cache.StringSet<string>(appCachekey, appHSSetting.ClientId, TimeSpan.FromMinutes(10));

            //缓存授权码取token时最长的有效时间
            DateTime expirTime = _cache.StringGet<DateTime>($"SessionExpiryKey:{sessionCode}");
            _cache.StringSet<DateTime>($"AuthCodeSessionTime:{code}", expirTime, expirTime - DateTime.Now);

            result.SetSuccess(code);
            return result;

        }

        /// <summary>
        /// 根据刷新Token获取Token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public string GetTokenByRefresh(string refreshToken, string clientId)
        {
            //刷新Token是否在缓存
            var currentUserModel = _cache.StringGet<CurrentUserModel>($"RefreshToken:{refreshToken}");
            if (currentUserModel == null)
            {
                return String.Empty;
            }
            //刷新token过期时间
            DateTime refreshTokenExpiry = _cache.StringGet<DateTime>($"RefreshTokenExpiry:{refreshToken}");
            //token默认时间为600s
            double tokenExpiry = 600;
            //如果刷新token的过期时间不到600s了，token过期时间为刷新token的过期时间
            if (refreshTokenExpiry > DateTime.Now && refreshTokenExpiry < DateTime.Now.AddSeconds(600))
            {
                tokenExpiry = (refreshTokenExpiry - DateTime.Now).TotalSeconds;
            }

            //从新生成Token
            string token = IssueToken(currentUserModel, clientId, tokenExpiry);
            return token;
        }

        /// <summary>
        /// 根据授权码,获取Token
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="appHSSetting"></param>
        /// <returns></returns>
        public ServerResponse<TokenResponse> GetTokenWithRefresh(string authCode)
        {
            var result = new ServerResponse<TokenResponse>();

            string key = $"AuthCode:{authCode}";
            string clientIdCachekey = $"AuthCodeClientId:{authCode}";
            string AuthCodeSessionTimeKey = $"AuthCodeSessionTime:{authCode}";

            //根据授权码获取用户信息
            var currentUserModel = _cache.StringGet<CurrentUserModel>(key);
            if (currentUserModel == null)
                throw new Exception("code无效");

            //清除authCode，只能用一次
            _cache.DeleteKey(key);

            //获取应用配置
            string clientId = _cache.StringGet<string>(clientIdCachekey);
            //刷新token过期时间
            DateTime sessionExpiryTime = _cache.StringGet<DateTime>(AuthCodeSessionTimeKey);
            DateTime tokenExpiryTime = DateTime.Now.AddMinutes(10);//token过期时间10分钟
            //如果刷新token有过期期比token默认时间短，把token过期时间设成和刷新token一样
            if (sessionExpiryTime > DateTime.Now && sessionExpiryTime < tokenExpiryTime)
            {
                tokenExpiryTime = sessionExpiryTime;
            }
            //获取访问token
            string token = this.IssueToken(currentUserModel, clientId, (sessionExpiryTime - DateTime.Now).TotalSeconds);

            TimeSpan refreshTokenExpiry;
            if (sessionExpiryTime != default(DateTime))
            {
                refreshTokenExpiry = sessionExpiryTime - DateTime.Now;
            }
            else
            {
                refreshTokenExpiry = TimeSpan.FromSeconds(60 * 60 * 24);//默认24小时
            }
            //获取刷新token
            string refreshToken = this.IssueToken(currentUserModel, clientId, refreshTokenExpiry.TotalSeconds);
            //缓存刷新token
            _cache.StringSet($"RefreshToken:{refreshToken}", currentUserModel, refreshTokenExpiry);
            //缓存刷新token过期时间
            _cache.StringSet($"RefreshTokenExpiry:{refreshToken}", DateTime.Now.AddSeconds(refreshTokenExpiry.TotalSeconds), refreshTokenExpiry);
            result.SetSuccess(new TokenResponse() { Token = token, RefreshToken = refreshToken, Expires = 60 * 10 });
            Console.WriteLine($"client_id:{clientId}获取token,有效期:{sessionExpiryTime.ToString("yyyy-MM-dd HH:mm:ss")},token:{token}");
            return result;
        }

        #region private methods

        /// <summary>
        /// 签发token
        /// </summary>
        /// <param name="userModel"></param>
        /// <param name="clientId"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private string IssueToken(CurrentUserModel userModel, string clientId, double seconds = 600)
        {
            var claims = new[]
            {
                   new Claim(ClaimTypes.Name, userModel.Name),
                   new Claim("Account", userModel.Account),
                   new Claim("Id", userModel.Id.ToString()),
                   new Claim("Mobile", userModel.Mobile),
                   new Claim(ClaimTypes.Role,userModel.Role),
            };

            var credentials = GetCredentials(clientId);

            /**
             * Claims (Payload)
                Claims 部分包含了一些跟这个 token 有关的重要信息。 JWT 标准规定了一些字段，下面节选一些字段:
                iss: The issuer of the token，签发主体，谁给的
                sub: The subject of the token，token 主题
                aud: 接收对象，给谁的
                exp: Expiration Time。 token 过期时间，Unix 时间戳格式
                iat: Issued At。 token 创建时间， Unix 时间戳格式
                jti: JWT ID。针对当前 token 的唯一标识
                除了规定的字段外，可以包含其他任何 JSON 兼容的字段。
             **/
            var token = new JwtSecurityToken(
                issuer: "SSO Center", //谁给的
                audience: clientId, //给谁的
                claims: claims,
                expires: DateTime.Now.AddSeconds(seconds),//token有效期
                notBefore: null,//立即生效  DateTime.Now.AddMilliseconds(30),//30s后有效
                signingCredentials: credentials);

            string returnToken = new JwtSecurityTokenHandler().WriteToken(token);
            return returnToken;
        }

        #endregion

        /// <summary>
        /// 获取加密方式
        /// </summary>
        /// <returns></returns>
        protected abstract SigningCredentials GetCredentials(string clientId);
    }
}
