using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TestJwt.Extensions;
using TestJwt.Tools;

var builder = WebApplication.CreateBuilder(args);

// 权限相关，注入JWT服务
builder.Services.AddJwt(builder.Configuration);

// 注入JwtTokenProvider
builder.Services.AddScoped<JwtTokenProvider, JwtTokenProvider>();

builder.Services.AddControllers()
    .AddNewtonsoftJson(option =>
    {
        option.SerializerSettings.ContractResolver = new DefaultContractResolver();
        option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
        option.SerializerSettings.Formatting = Formatting.Indented;
        option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

// 注册swagger 3.x
builder.Services.AddKnife4jSwagger(builder.Configuration, exclude_xmlNames: null);

// 注册IHttpClientFactory
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseKnife4jSwagger(builder.Configuration);
}

app.UseHttpsRedirection();
app.UseRouting();

// 启用认证中间件。
app.UseAuthentication();
// 启用授权中间件
app.UseAuthorization();

// 配置路由
app.MapControllers();

app.Run();