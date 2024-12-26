using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TestJwt.Tools
{
    /// <summary>
    /// JWT配置类
    /// </summary>
    public class JwtOption
    {
        /// <summary>
        /// 目标受众
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// 安全密钥
        /// </summary>
        public string SecurityKey { get; set; }

        /// <summary>
        /// 签发方
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// 获取JWT配置
        /// </summary>
        /// <remarks>优先从配置文件加载，其次再使用默认值</remarks>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static JwtOption GetJwtOptions(IConfiguration configuration)
        {
            //从配置中获取动态配置
            var dynamic_jwtOption = configuration.GetSection("Jwt").Get<JwtOption>();
            if (dynamic_jwtOption == null || string.IsNullOrWhiteSpace(dynamic_jwtOption.SecurityKey))
            {
                Console.WriteLine("JWT SecurityKey is required but not found in the configuration.");
                //throw new InvalidOperationException("JWT SecurityKey is required but not found in the configuration.");
                return new JwtOption
                {
                    Audience = "Client",
                    Issuer = "Enrich",
                    SecurityKey = "ASJStaticXgr8Bao8Ae8vs9y4gryNiWM8RC305i8yvUYCgRI7rHa7xJZqa9bzYFwog5x1iQ7l3L0YxaYSc4GluYT",
                };
            }
            else
            {
                return dynamic_jwtOption;
            }
        }

        /// <summary>
        /// 获取默认的令牌验证参数。
        /// </summary>
        /// <param name="option"></param>
        public static TokenValidationParameters GetTokenValidationParameters(JwtOption option)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true, // 验证签发方
                ValidateAudience = true, // 验证目标受众
                ValidateLifetime = true, // 验证令牌的有效期
                ValidateIssuerSigningKey = true, // 验证签名密钥
                ValidIssuer = option.Issuer, // 设置有效的签发方
                ValidAudience = option.Audience, // 设置有效的目标受众
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.SecurityKey)), // 设置签名密钥
                ClockSkew = TimeSpan.FromSeconds(10) // 允许的时间偏移量，以避免由于服务器时间不同步导致的问题
            };
        }
    }
}