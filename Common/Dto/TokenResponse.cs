using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    /// <summary>
    /// 获取token响应结果
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 刷新token
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// 过期时间,多少s后
        /// </summary>
        public double Expires { get; set; }
        /// <summary>
        /// 资源域
        /// </summary>
        public string Scope { get; set; }
    }
}
