namespace TestJwt.Tools
{
    /// <summary>
    /// 当前用户
    /// </summary>
    public class CurrentUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// 登录平台
        /// </summary>
        public string LoginPlatform { get; set; } = string.Empty;



        public CurrentUser(Guid userID, string loginPlatform)
        {
            UserID = userID;
            LoginPlatform = loginPlatform;
        }

    }
}
