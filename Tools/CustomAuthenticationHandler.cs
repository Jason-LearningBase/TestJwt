using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace TestJwt.Tools
{
    /// <summary>
    /// 自定义身份验证处理器。该处理器负责处理身份验证挑战（401 Unauthorized）和禁止访问（403 Forbidden）响应。
    /// </summary>
    public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtTokenProvider _jwtTokenProvider;

        /// <summary>
        /// 构造函数，初始化身份验证处理器。
        /// </summary>
        /// <param name="options">身份验证选项。</param>
        /// <param name="logger">日志记录工厂。</param>
        /// <param name="encoder">URL编码器。</param>
        /// <param name="jwtTokenProvider">JWT Token 提供类。</param>
        public CustomAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, JwtTokenProvider jwtTokenProvider)
            : base(options, logger, encoder)
        {
            _jwtTokenProvider = jwtTokenProvider;
        }

        /// <summary>
        /// 处理身份验证逻辑。此方法可以根据自己的需求进行自定义开发
        /// </summary>
        /// <returns>认证结果。</returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("No JWT token found");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            try
            {
                DateTime now = DateTime.UtcNow;
                // 是否过期
                var time = _jwtTokenProvider.TryGetExpirationTime(token, out now);
                Guid? userId;
                string loginPlatform = string.Empty;
                // 验证 JWT token
                var is_principal_ok = _jwtTokenProvider.TryGetClaim(token, out userId, out loginPlatform);

                if (!is_principal_ok)
                {
                    return AuthenticateResult.Fail("Invalid token");
                }
                if (DateTime.Now > now)
                {
                    return AuthenticateResult.Fail("token已过期");
                }
                // 初始化一个 ClaimsPrincipal
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();

                claimsPrincipal.AddIdentity(new ClaimsIdentity(new[]
                {
                  new Claim(JwtClaimTypes.UserID, userId.ToString()),
                  new Claim(JwtClaimTypes.LoginPlatform, loginPlatform)
               }, JwtBearerDefaults.AuthenticationScheme));
                // 跳过角色验证，允许访问
                var ticket = new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme);
                // 返回认证成功
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
            }
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