namespace Web1.Models
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    public class AppSettingsOptions
    {
        public AppHSSetting AppHSSetting { get; set; }

        public AppRSSetting AppRSSetting { get; set; }
    }

    /// <summary>
    /// 应用配置-对称加密方式
    /// </summary>
    public class AppHSSetting
    {
        /// <summary>
        /// 验证Issuer
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 验证Audience
        /// </summary>
        public string Audience { get; set; }
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
        /// 验证Issuer
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 验证Audience
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 应用Key
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 公钥-加密
        /// </summary>
        public string PublicKey { get; set; }
    }
}
