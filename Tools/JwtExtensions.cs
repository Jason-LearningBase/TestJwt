using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using TestJwt.Tools;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtExtensions
    {
        //public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //        .AddJwtBearer(options =>
        //        {
        //            options.RequireHttpsMetadata = false;
        //            options.SaveToken = true;
        //            options.TokenValidationParameters = new TokenValidationParameters()
        //            {
        //                ValidateIssuer = false,  // 是否验证 Issuer
        //                ValidateAudience = false,  // 是否验证 Audience
        //                ValidateLifetime = false,  // 是否验证生命周期（过期时间）
        //                ValidateIssuerSigningKey = true,  // 是否验证 SecurityKey
        //                ValidIssuer = "Enrich",  // 有效的 Issuer
        //                ValidAudience = "Client",  // 有效的 Audience
        //                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ASJStaticXgr8Bao8Ae8vs9y4gryNiWM8RC305i8yvUYCgRI7rHa7xJZqa9bzYFwog5x1iQ7l3L0YxaYSc4GluYT"))
        //            };

        //            options.Events = new JwtBearerEvents
        //            {
        //                //此处为权限验证失败后触发的事件
        //                OnChallenge = context =>
        //                {

        //                    //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
        //                    context.HandleResponse();

        //                    //自定义自己想要返回的数据结果，我这里要返回的是Json对象，通过引用Newtonsoft.Json库进行转换
        //                    var payload = JsonConvert.SerializeObject(new { code = -403, msg = "认证不通过/Status401Unauthorized" });
        //                    //自定义返回的数据类型
        //                    context.Response.ContentType = "application/json";
        //                    //自定义返回状态码，默认为401 我这里改成 200
        //                    context.Response.StatusCode = StatusCodes.Status200OK;
        //                    //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //                    //输出Json数据结果
        //                    context.Response.WriteAsync(payload);
        //                    return Task.FromResult(0);
        //                },
        //                OnAuthenticationFailed = context =>
        //                {
        //                    //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
        //                    //context.HandleResponse();

        //                    //自定义自己想要返回的数据结果，我这里要返回的是Json对象，通过引用Newtonsoft.Json库进行转换
        //                    var payload = JsonConvert.SerializeObject(new { code = -403, msg = "认证不通过/Status401Unauthorized" });
        //                    //自定义返回的数据类型
        //                    context.Response.ContentType = "application/json";
        //                    //自定义返回状态码，默认为401 我这里改成 200
        //                    context.Response.StatusCode = StatusCodes.Status200OK;
        //                    //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //                    //输出Json数据结果
        //                    context.Response.WriteAsync(payload);
        //                    return Task.FromResult(0);
        //                },
        //                OnForbidden = context =>
        //                {
        //                    //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
        //                    //context.HandleResponse();

        //                    //自定义自己想要返回的数据结果，我这里要返回的是Json对象，通过引用Newtonsoft.Json库进行转换
        //                    var payload = JsonConvert.SerializeObject(new { code = -403, msg = "认证不通过/Status401Unauthorized" });
        //                    //自定义返回的数据类型
        //                    context.Response.ContentType = "application/json";
        //                    //自定义返回状态码，默认为401 我这里改成 200
        //                    context.Response.StatusCode = StatusCodes.Status200OK;
        //                    //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //                    //输出Json数据结果
        //                    context.Response.WriteAsync(payload);
        //                    return Task.FromResult(0);
        //                }
        //            };
        //        });

        //    return services;
        //}




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
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = nameof(CustomAuthenticationHandler); // 401 未认证
                options.DefaultForbidScheme = nameof(CustomAuthenticationHandler);  // 403 禁止
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
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
