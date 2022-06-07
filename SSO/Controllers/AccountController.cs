using Common;
using Common.Cache;
using Common.Dto;
using Microsoft.AspNetCore.Mvc;
using SSO.Services;
using System.Text;
using System.Text.Json;

namespace SSO.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJWTService _jwtService;
        private readonly ICacheService _cacheService;

        public AccountController(IHttpClientFactory httpClientFactory, IJWTService jwtService, ICacheService cacheService)
        {
            _httpClientFactory = httpClientFactory;
            _jwtService = jwtService;
            _cacheService = cacheService;
        }

        /// <summary>
        /// 登录页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 退出登录页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }

        /// <summary>
        /// 获取授权码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ServerResponse<string> GetCode([FromBody] CodeRequest request)
        {
            var result = _jwtService.GetCode(request.ClientId, request.Username, request.Password);
            return result;
        }

        /// <summary>
        /// 根据会话code, 获取授权码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ServerResponse<string> GetCodeBySessionCode([FromBody] SessionCodeRequest request)
        {
            var result = _jwtService.GetCodeBySessionCode(request.ClientId, request.SessionCode);
            return result;
        }

        /// <summary>
        /// 根据授权码, 获取Token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ServerResponse<TokenResponse> GetToken([FromBody] TokenRequest request)
        {
            var response = _jwtService.GetTokenWithRefresh(request.AuthCode);
            return response;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServerResponse> LogOutApp([FromBody] LogoutRequest request)
        {
            //删除全局会话
            string sessionKey = $"SessionCode:{request.SessionCode}";
            _cacheService.DeleteKey(sessionKey);

            var client = _httpClientFactory.CreateClient();

            var param = new { sessionCode = request.SessionCode };
            string jsonData = JsonSerializer.Serialize(param, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            //这里实战中是用数据库或缓存取
            var urls = new List<string>()
            {
                "http://localhost:7001/Account/Logout",
                "http://localhost:7002/Account/Logout"
            };

            //这里可以异步mq处理，不阻塞返回
            foreach (var url in urls)
            {
                // web 退出登录
                var logOutResponse = await client.PostAsync(url, new StringContent(jsonData, Encoding.UTF8, "application/json"));
                string resultStr = await logOutResponse.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<ServerResponse>(resultStr, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                if (response.Code == 0) //成功
                {
                    Console.WriteLine($"url:{url},会话Id:{request.SessionCode},退出登录成功");
                }
                else
                {
                    Console.WriteLine($"url:{url},会话Id:{request.SessionCode},退出登录失败");
                }
            };
            return new ServerResponse().SetSuccess();
        }
    }
}
