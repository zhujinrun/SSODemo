using Common;
using Common.Dto;

namespace SSO.Services
{
    /// <summary>
    /// JWT服务接口
    /// </summary>
    public interface IJWTService
    {
        /// <summary>
        /// 获取授权码
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        ServerResponse<string> GetCode(string clientId, string userName, string password);

        /// <summary>
        /// 根据会话Code获取授权码
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="sessionCode"></param>
        /// <returns></returns>
        ServerResponse<string> GetCodeBySessionCode(string clientId, string sessionCode);

        /// <summary>
        /// 获取Token+RefreshToken
        /// </summary>
        /// <param name="authCode"></param>
        /// <returns>Token+RefreshToken</returns>
        ServerResponse<TokenResponse> GetTokenWithRefresh(string authCode);

        /// <summary>
        /// 基于refreshToken获取Token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        string GetTokenByRefresh(string refreshToken, string clientId);
    }
}
