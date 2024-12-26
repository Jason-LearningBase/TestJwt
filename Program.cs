using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TestJwt.Extensions;
using TestJwt.Tools;

var builder = WebApplication.CreateBuilder(args);

// Ȩ����أ�ע��JWT����
builder.Services.AddJwt(builder.Configuration);

// ע��JwtTokenProvider
builder.Services.AddScoped<JwtTokenProvider, JwtTokenProvider>();

builder.Services.AddControllers()
    .AddNewtonsoftJson(option =>
    {
        option.SerializerSettings.ContractResolver = new DefaultContractResolver();
        option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
        option.SerializerSettings.Formatting = Formatting.Indented;
        option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

// ע��swagger 3.x
builder.Services.AddKnife4jSwagger(builder.Configuration, exclude_xmlNames: null);

// ע��IHttpClientFactory
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

// ������֤�м����
app.UseAuthentication();
// ������Ȩ�м��
app.UseAuthorization();

// ����·��
app.MapControllers();

app.Run();