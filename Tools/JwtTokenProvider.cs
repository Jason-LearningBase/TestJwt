using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TestJwt.Tools
{
    /// <summary>
    /// JWT Token 提供类
    /// 提供生成、验证和解析JWT Token的功能
    /// </summary>
    public class JwtTokenProvider
    {
        private readonly JwtOption _options;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly TokenValidationParameters _validationParameters;
        private readonly ILogger<JwtTokenProvider> _logger;

        /// <summary>
        /// JwtTokenProvider构造函数，通过依赖注入获取配置选项
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger">日志记录器</param>
        public JwtTokenProvider(IConfiguration configuration, ILogger<JwtTokenProvider> logger)
        {
            _logger = logger;
            _options = JwtOption.GetJwtOptions(configuration);
            _validationParameters = JwtOption.GetTokenValidationParameters(JwtOption.GetJwtOptions(configuration));
            _tokenHandler = new JwtSecurityTokenHandler();
        }


        /// <summary>
        /// 生成JWT Token
        /// </summary>
        /// <param name="user">当前用户对象</param>
        /// <param name="expMins">令牌有效期（分钟）</param>
        /// <returns>生成的JWT Token</returns>
        private string Generate(CurrentUser user, int expMins)
        {
            // 存入Token的信息
            var claims = new[]
            {
                new Claim(JwtClaimTypes.UserID, user.UserID.ToString()),
                new Claim(JwtClaimTypes.LoginPlatform, user.LoginPlatform)
            };

            // 加密key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 生成Token
            var securityToken = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expMins),
                signingCredentials: creds);

            // 将Token信息转换为字符串
            return _tokenHandler.WriteToken(securityToken);
        }

        /// <summary>
        /// 根据用户信息生成JWT Token
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="loginPlatform">平台</param>
        /// <param name="expMins">令牌有效期（分钟）</param>
        /// <returns>生成的JWT Token</returns>
        public string GenerateToken(Guid userID, string loginPlatform, int expMins = 60)
        {
            return Generate(new CurrentUser(userID, loginPlatform), expMins);
        }

        /// <summary>
        /// 获取JWT Token的过期时间
        /// </summary>
        /// <param name="jwtToken">JWT Token字符串</param>
        /// <param name="expirationTime">输出的过期时间</param>
        /// <param name="getLocalTime">是否返回本地时间</param>
        /// <returns>是否成功获取过期时间</returns>
        public bool TryGetExpirationTime(string jwtToken, out DateTime expirationTime, bool getLocalTime = true)
        {
            try
            {
                // 尝试解析并验证JWT Token
                var claimsPrincipal = _tokenHandler.ValidateToken(jwtToken, _validationParameters, out SecurityToken validatedToken);
                var jwtSecurityToken = validatedToken as JwtSecurityToken;

                // 获取过期时间
                expirationTime = jwtSecurityToken?.ValidTo ?? DateTime.MinValue;

                // 根据需要转换为本地时间
                expirationTime = getLocalTime
                    ? TimeZoneInfo.ConvertTimeFromUtc(expirationTime, TimeZoneInfo.Local)
                    : expirationTime;

                return expirationTime != DateTime.MinValue; // 如果解析成功，返回true，否则返回false
            }
            catch (Exception)
            {
                // 如果解析失败，设置为默认时间并返回false
                expirationTime = DateTime.MinValue;
                return false;
            }
        }

        /// <summary>
        /// 获取JWT Token的过期时间戳（Unix时间戳）
        /// </summary>
        /// <param name="jwtToken">JWT Token字符串</param>
        /// <param name="expirationTimestamp">输出的过期时间戳（Unix时间戳）</param>
        /// <returns>是否成功获取过期时间戳</returns>
        public bool TryGetExpirationTimestamp(string jwtToken, out long? expirationTimestamp)
        {
            try
            {
                // 尝试解析并验证JWT Token
                var claimsPrincipal = _tokenHandler.ValidateToken(jwtToken, _validationParameters, out SecurityToken validatedToken);
                var jwtSecurityToken = validatedToken as JwtSecurityToken;

                // 获取过期时间戳（Unix时间戳）
                expirationTimestamp = jwtSecurityToken?.Payload.Expiration;

                // 如果无法获取，返回 null
                return expirationTimestamp.HasValue;
            }
            catch (Exception)
            {
                // 如果解析失败，返回 null
                expirationTimestamp = null;
                return false;
            }
        }

        /// <summary>
        /// 获取JWT Token的Claim信息
        /// </summary>
        /// <param name="jwtToken">JWT Token字符串</param>
        /// <param name="userID">输出用户ID</param>
        /// <param name="loginPlatform">输出平台</param>
        /// <returns>是否成功获取Claim信息</returns>
        public bool TryGetClaim(string jwtToken, out Guid? userID, out string? loginPlatform)
        {
            try
            {
                var claimsPrincipal = _tokenHandler.ValidateToken(jwtToken, _validationParameters, out SecurityToken validatedToken);
                var jwtSecurityToken = validatedToken as JwtSecurityToken;
                var claims = jwtSecurityToken?.Claims;
                // 提取UserID
                var userIdStr = claims?.FirstOrDefault(t => t.Type == JwtClaimTypes.UserID)?.Value;
                userID = string.IsNullOrWhiteSpace(userIdStr) ? null : (Guid?)Guid.Parse(userIdStr);
                // 提取LoginPlatform
                loginPlatform = claims?.FirstOrDefault(t => t.Type == JwtClaimTypes.LoginPlatform)?.Value;
                return userID.HasValue && !string.IsNullOrEmpty(loginPlatform); // 返回是否成功解析
            }
            catch (SecurityTokenExpiredException)
            {
                // 忽略过期token的异常
                userID = null;
                loginPlatform = null;
                return false;
            }
            catch (Exception ex)
            {
                // 捕获其他异常并记录
                _logger.LogError(ex, "Failed to parse JWT claims.");
                userID = null;
                loginPlatform = null;
                return false;
            }

        }



    }
}
