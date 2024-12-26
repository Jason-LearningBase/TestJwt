using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace TestJwt.Extensions
{
    /// <summary>
    /// Swagger扩展
    /// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// 添加Knife4j的Swagger服务配置
        /// </summary>
        /// <param name="services">服务容器</param> 
        /// <param name="configuration">配置</param>
        /// <param name="exclude_xmlNames">要忽略的XML文档名称列表</param>
        /// <returns>services</returns>
        public static IServiceCollection AddKnife4jSwagger(this IServiceCollection services, IConfiguration configuration, IEnumerable<string>? exclude_xmlNames = null)
        {
            // 注册 SwaggerGen 服务
            services.AddSwaggerGen(options =>
            {
                // 添加主 Swagger 文档的名称和元数据
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sinno MCM",
                    Version = "v1",
                    Description = "Sinno MCM API",
                });

                options.EnableAnnotations(); // 启用注释支持

                // 添加服务器信息(仅用于展示)
                options.AddServer(new OpenApiServer()
                {
                    Url = "http://localhost:39001/swagger/index.html",
                    Description = "MCM Api Docs"
                });


                // 向 Swagger 文档添加一个名为 "Bearer" 的安全定义
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    // 描述如何使用JWT认证
                    Description = "JWT 认证授权请求头. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",  // HTTP头部字段名称
                    In = ParameterLocation.Header,  // 指定安全信息位于HTTP头部
                    Type = SecuritySchemeType.ApiKey,  // 安全模式类型为API密钥
                    BearerFormat  = "JWT",  // JWT格式
                    Scheme = "bearer"  // 认证方案名称
                });

                // 将上述定义的安全方案应用于整个 API 或者特定的操作上
                // Notice: 推荐全部使用，安全性高一点；如果相单独使用，可以注释这里，并在需要的控制器上添加[Authorize]
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });

                // 添加自定义 Swagger 节点
                HashSet<string> addedGroupNames = []; // 用于去重
                foreach (var controller in GetControllers())
                {
                    var groupName = GetGroupName(controller);
                    if (!addedGroupNames.Contains(groupName))
                    {
                        options.SwaggerDoc(groupName, new OpenApiInfo
                        {
                            Version = "v1",
                            Title = groupName,
                        });
                        addedGroupNames.Add(groupName);
                    }
                }

                // API端点按操作名称排序，暂时不需要，还是按照代码中从上到下的编写顺序效果更好
                //options.OrderActionsBy(apiDescription =>
                //{
                //    var actionName = apiDescription.ActionDescriptor.RouteValues["action"];
                //    return actionName;
                //});

                // 引入导出的XML注释文件, Swagger文档将能够展示控制器和操作上的注释描述
                foreach (var filePath in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                {
                    var fileFullName = (filePath.Split('\\'))[^1]; //xxx.xml
                    if (exclude_xmlNames == null || fileFullName.Contains("WebHost") || fileFullName.Contains("Share") || fileFullName.Contains("Infrastructure") || !exclude_xmlNames.Any(t => fileFullName.Contains(t)))
                    {
                        options.IncludeXmlComments(filePath, true);
                    }
                }


                // 使用自定义过滤器，设置对象类型参数默认值
                options.SchemaFilter<CombinedSchemaFilter>();
                // 为枚举类型生成内联定义，不生成schema类，实现代码和接口更紧凑清晰
                options.UseInlineDefinitionsForEnums();


                // 为每个操作生成唯一ID
                options.CustomOperationIds(apiDesc =>
                {
                    if (apiDesc==null || apiDesc.ActionDescriptor == null)
                    {
                        // 日志记录
                        Console.WriteLine("ActionDescriptor is null");
                        return Guid.NewGuid().ToString();
                    }

                    if (apiDesc.ActionDescriptor is ControllerActionDescriptor controllerAction)
                    {
                        return $"{controllerAction.ControllerName}-{controllerAction.ActionName}";
                    }
                    else if (apiDesc.ActionDescriptor is ActionDescriptor actionDescriptor)
                    {
                        return $"MCM-{apiDesc.RelativePath?.Replace("/", "-")}"; // 默认处理
                    }
                    else
                    {
                        return Guid.NewGuid().ToString();
                    }
                });

                // 添加过滤器
                options.DocumentFilter<HideApiEndpointsFilter>(); 

            });

            // 设置URL路径自动转换为小写, 格式统一、避免重复
            services.AddRouting(o => o.LowercaseUrls = true);

            // 关闭对安全性元数据未处理检查的警告, 开发阶段设置false,即保留警告作为反馈
            services.AddRouting(o => o.SuppressCheckForUnhandledSecurityMetadata = false);

            return services;
        }


        /// <summary>
        /// 获取所有控制器
        /// </summary>
        public static List<Type> GetControllers()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // 获取所有继承自 ControllerBase 的类型
            var controllerTypes = assemblies
              .SelectMany(x => x.GetExportedTypes())
              .Where(x => typeof(ControllerBase).IsAssignableFrom(x) && !x.IsAbstract && !x.IsGenericType);

            // 默认排在首位的控制器名称
            var defaultName = "AuthController";  // 还可以是 "SystemController"

            // 将控制器按名称排序，并确保默认控制器始终位于首位
            var sortedControllers = controllerTypes.OrderBy(x => x.Name != defaultName ? x.Name : string.Empty);

            // 返回排序后的控制器类型列表
            return [.. sortedControllers];
        }


        /// <summary>
        /// 获取控制器的分组名称
        /// </summary>
        public static string GetGroupName(Type controllerType)
        {
            var controllerGroupName = controllerType.Name.Replace("Controller", "");

            var groupNameAttribute = controllerType
              .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute))
              .FirstOrDefault();

            if (groupNameAttribute != null)
            {
                controllerGroupName = ((ApiExplorerSettingsAttribute)groupNameAttribute).GroupName;
            }

            return controllerGroupName??"";
        }


        /// <summary>
        /// 添加Knife4jSwagger中间件配置
        /// </summary>
        /// <param name="app">应用</param>
        /// <param name="configuration">配置</param>
        /// <returns>app</returns>
        public static IApplicationBuilder UseKnife4jSwagger(this IApplicationBuilder app, IConfiguration configuration)
        {
            // 添加Swagger有关中间件
            app.UseSwagger();
            // 添加Knife4jSwagger中间件
            app.UseKnife4UI(options =>
            {
                // 指定Swagger UI的根路径
                options.RoutePrefix = "swagger";

                // 添加Swagger端点
                //options.SwaggerEndpoint("/v1/swagger.json", "MCM.webapi v1");

                // 添加其他Swagger端点, 循环遍历所有控制器，添加Swagger端点
                HashSet<string> addedGroupNames = []; // 用于去重
                foreach (var controller in GetControllers())
                {
                    var groupName = GetGroupName(controller);
                    if (!addedGroupNames.Contains(groupName))
                    {
                        options.SwaggerEndpoint($"/{groupName}/swagger.json", groupName);
                        addedGroupNames.Add(groupName);
                    }
                }
            });
            return app;
        }

        /// <summary>
        /// 排除特定列表的controller在API文档不显示(控制器已经加载至程序内)
        /// </summary>
        public class ActionHidingConvention : IActionModelConvention
        {
            /// <summary>
            /// 要隐藏的controller名称列表
            /// </summary>
            private readonly IEnumerable<string> _controllersToHide;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="controllersToHide">要隐藏的controller名称列表</param>
            public ActionHidingConvention(IEnumerable<string> controllersToHide) => _controllersToHide = controllersToHide;

            public void Apply(ActionModel action)
            {
                var controllerName = action.Controller.ControllerName;

                // 隐藏匹配名称的控制器
                if (_controllersToHide.Contains(controllerName))
                {
                    action.ApiExplorer.IsVisible = false;
                }
            }
        }

        /// <summary>
        /// 隐藏指定路径的API
        /// </summary>
        public class HideApiEndpointsFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                // 移除 /hello 接口
                var pathsToRemove = new[] { "/hello" };
                foreach (var path in pathsToRemove)
                {
                    swaggerDoc.Paths.Remove(path);
                }
            }
        }


        /// <summary>
        /// 自定义过滤器，实现多种类型的默认值处理
        /// </summary>

        public class CombinedSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                //读取失败就不处理
                if (schema == null)
                {
                    return;
                }

                // 处理枚举类型
                if (context.Type.IsEnum)
                {
                    schema.Enum.Clear();
                    Enum.GetNames(context.Type).ToList().ForEach(name =>
                    {
                        Enum e = (Enum)Enum.Parse(context.Type, name);
                        schema.Enum.Add(new OpenApiString("(" + $"{name} : {Convert.ToInt64(Enum.Parse(context.Type, name))}" + ")"));
                    });
                }
                // 处理 Guid 类型
                else if (context.Type == typeof(Guid))
                {
                    schema.Example = new OpenApiString("00000000-0000-0000-0000-000000000000");
                }
                // 处理 Nullable Guid 类型
                else if (context.Type == typeof(Guid?))
                {
                    schema.Format = "uuid | null";
                    schema.Default = new OpenApiNull();
                    schema.Nullable = true;

                    // 不要设置示例值，会覆盖默认值
                    //schema.Example = new OpenApiString("00000000-0000-0000-0000-000000000000"); // 不需要设置 Example 为特定值
                    //schema.Example = new OpenApiNull(); // 设置 Example 为 null，不太理想，Swagger会显示"null"
                    //如果需要提醒uuid的格式，可以修改描述，但其实不需要，界面也丑陋
                    //schema.Description += " 可以为：( 00000000-0000-0000-0000-000000000000 )";
                }
                // 处理字符串类型的默认值
                else if (context.Type == typeof(string))
                {
                    //如果为非必填或可为null，则设置默认值
                    if (!schema.Required.Any() && !schema.Nullable)
                    {
                        schema.Default = new OpenApiString("");
                    }
                }

                //处理其他类型
                if (schema.Properties != null && context!= null)
                {
                    //处理pageIndex
                    if (schema.Properties.TryGetValue("pageIndex", out OpenApiSchema? value))
                    {
                        value.Default = new OpenApiInteger(1);
                    }
                    //处理pageSize
                    if (schema.Properties.TryGetValue("pageSize", out OpenApiSchema? value2))
                    {
                        value2.Default = new OpenApiInteger(10);
                    }

                    //通过特性来实现
                    DefaultValueAttribute? defaultValue = context.ParameterInfo?.GetCustomAttribute<DefaultValueAttribute>();
                    if (defaultValue != null)
                    {
                        schema.Default = (IOpenApiAny)(defaultValue.Value??"");
                    }
                }

            }
        }


    }
}
