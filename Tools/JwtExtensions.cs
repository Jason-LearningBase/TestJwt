using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using TestJwt.Tools;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// JWT扩展
    /// </summary>
    public static class JwtExtensions
    {
        /// <summary>
        /// 向服务容器中添加JWT身份验证支持
        /// </summary>
        /// <param name="services">ASP.NET Core 服务集合。</param>
        /// <param name="configuration">配置对象，包含JWT设置。</param>
        /// <returns>返回修改后的服务集合。</returns>
        public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置身份验证服务，设置默认的身份验证和挑战方案
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = nameof(CustomAuthenticationHandler);
                options.DefaultChallengeScheme = nameof(CustomAuthenticationHandler); // 401 未认证
                options.DefaultForbidScheme = nameof(CustomAuthenticationHandler);  // 403 禁止
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                // 获取JWT配置并设置Token验证参数
                options.TokenValidationParameters = JwtOption.GetTokenValidationParameters(JwtOption.GetJwtOptions(configuration));
            })
            // 添加自定义的身份验证方案
            .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(nameof(CustomAuthenticationHandler), o => { });

            return services;
        }
    }
}
