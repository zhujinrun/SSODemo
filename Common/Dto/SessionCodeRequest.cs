namespace Common.Dto
{
    public class SessionCodeRequest
    {
        /// <summary>
        /// 应用Id
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 会话code
        /// </summary>
        public string SessionCode { get; set; }
    }
}
