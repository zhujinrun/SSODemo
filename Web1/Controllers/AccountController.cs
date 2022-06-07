using Common;
using Common.Cache;
using Common.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Web1.Models;

namespace Web1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICacheService _cacheService;

        public AccountController(IHttpClientFactory httpClientFactory, ICacheService cacheService)
        {
            _httpClientFactory = httpClientFactory;
            _cacheService = cacheService;
        }

        /// <summary>
        /// 登录成功回调
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LoginRedirect()
        {
            return View();
        }

        /// <summary>
        /// 获取用户信息，接口需要进行权限校验
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        [HttpPost]
        public ServerResponse<UserModel> GetUserInfo()
        {
            return new ServerResponse<UserModel>();
        }

        /// <summary>
        /// 根据authCode, 获取token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServerResponse<TokenResponse>> GetAccessCode([FromBody] AccessCodeRequest request)
        {
            //请求SSO获取 token
            var client = _httpClientFactory.CreateClient();
            var param = new { authCode = request.AuthCode };
            string jsonData = JsonSerializer.Serialize(param, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var response = await client.PostAsync("http://localhost:7000/Account/GetToken", new StringContent(jsonData, Encoding.UTF8, "application/json"));
            string resultStr = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ServerResponse<TokenResponse>>(resultStr, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (result.Code == 0) //成功
            {
                //成功,缓存token到局部会话
                _cacheService.StringSet<string>($"SessionCode:{request.SessionCode}", result.Data.Token, TimeSpan.FromSeconds(result.Data.Expires));
                _cacheService.StringSet<bool>($"token:{result.Data.Token}", true, TimeSpan.FromSeconds(result.Data.Expires));
                Console.WriteLine($"获取token成功，局部会话code:{request.SessionCode},{Environment.NewLine}token:{result.Data.Token}");
            }
            return result;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ServerResponse LogOut([FromBody] LogoutRequest request)
        {
            //根据会话取出token
            string token = _cacheService.StringGet<string>($"SessionCode:{request.SessionCode}");
            if (!string.IsNullOrEmpty(token))
            {
                //清除token
                _cacheService.DeleteKey($"token:{token}");
            }
            Console.WriteLine($"会话Code:{request.SessionCode}退出登录");
            return new ServerResponse().SetSuccess();
        }
    }
}
