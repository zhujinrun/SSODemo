namespace SSO.Models
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    public class AppSettingsOptions
    {
        public double TokenExpireSeconds { get; set; }

        public double RefreshTokenExpireSeconds { get; set; }

        public List<AppHSSetting> AppHSSettings { get; set; }

        public List<AppRSSetting> AppRSSettings { get; set; }
    }

    /// <summary>
    /// 应用配置-对称加密方式
    /// </summary>
    public class AppHSSetting
    {
        /// <summary>
        /// 应用域名
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// 应用Key
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 应用密钥
        /// </summary>
        public string ClientSecret { get; set; }
    }

    /// <summary>
    /// 应用配置-非对称加密
    /// </summary>
    public class AppRSSetting
    {
        /// <summary>
        /// 应用域名
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// 应用Key
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 私钥-解密
        /// </summary>
        public string PrivateKey { get; set; }
    }
}
