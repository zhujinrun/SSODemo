namespace SSO.Models
{
    public class SessionCodeUser
    {
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiresTime { get; set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public CurrentUserModel CurrentUser { get; set; }
    }

    public class CurrentUserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string Mobile { get; set; }
        public string Role { get; set; }
    }
}
