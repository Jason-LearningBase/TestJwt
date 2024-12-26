using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System;

namespace TestJwt.Tools
{
    /// <summary>
    /// 自定义身份验证处理器。该处理器负责处理身份验证挑战（401 Unauthorized）和禁止访问（403 Forbidden）响应。
    /// </summary>
    public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// 构造函数，初始化身份验证处理器。
        /// </summary>
        /// <param name="options">身份验证选项。</param>
        /// <param name="logger">日志记录工厂。</param>
        /// <param name="encoder">URL编码器。</param>
        public CustomAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder) { }

        /// <summary>
        /// 处理身份验证逻辑。此方法未实现，因此抛出未实现异常。
        /// </summary>
        /// <returns>认证结果。</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            throw new NotImplementedException("此处理器未实现认证逻辑。");
        }

        /// <summary>
        /// 处理认证失败的情况，返回 401 未认证错误。
        /// </summary>
        /// <param name="properties">认证属性。</param>
        /// <returns>异步任务。</returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.ContentType = "application/json";
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            var errMsg = JsonConvert.SerializeObject(new { code = 401, msg = "认证失败" });
            await Response.WriteAsync(errMsg);
        }

        /// <summary>
        /// 处理禁止访问的情况，返回 403 禁止访问错误。
        /// </summary>
        /// <param name="properties">认证属性。</param>
        /// <returns>异步任务。</returns>
        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.ContentType = "application/json";
            Response.StatusCode = StatusCodes.Status403Forbidden;
            var errMsg = JsonConvert.SerializeObject(new { code = 403, msg = "禁止访问：权限不足" });
            await Response.WriteAsync(errMsg);
        }
    }
}