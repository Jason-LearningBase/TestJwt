

# .NET Core + JWT 测试项目

## 项目简介

这是一个简单的测试项目，旨在演示如何在 ASP.NET Core 应用程序中实现 JSON Web Token (JWT) 身份验证。通过这个项目，你可以了解如何配置 JWT 认证、生成和解析 JWT 令牌，以及如何保护 API 端点。

## 目录结构

```
.
├── Controllers
│   └── SystemInfoController.cs          // 包含公开和受保护的 API 方法
├── Extensions
│   └── SwaggerExtensions.cs             // Swagger 配置扩展方法
├── Tools
│   ├── CurrentUser.cs                   // 当前用户信息类
│   ├── CustomAuthenticationHandler.cs    // 自定义认证处理器
│   ├── JwtClaimTypes.cs                 // JWT 声明类型
│   ├── JwtExtensions.cs                 // JWT 扩展方法
│   ├── JwtOption.cs                     // JWT 选项配置
│   └── JwtTokenProvider.cs              // JWT 令牌提供者类
├── Properties
│   └── launchSettings.json              // 启动设置文件
├── appsettings.json                     // 应用程序配置文件
├── appsettings.Development.json         // 开发环境配置文件
├── Program.cs                           // 应用程序入口点
├── README.md                            // 本文件
├── TestJwt.http                         // HTTP 请求测试脚本
```

## 功能特性

- **JWT 身份验证**：使用 `Microsoft.AspNetCore.Authentication.JwtBearer` 和 `Microsoft.IdentityModel.Tokens` 实现。
- **API 保护**：部分 API 端点需要有效的 JWT 才能访问。
- **匿名访问支持**：特定端点允许未认证用户访问（例如欢迎页面）。
- **自定义错误响应**：为认证失败和授权失败提供定制化的 JSON 响应。
- **Swagger UI**：集成 Knife4j Swagger 提供 API 文档和测试界面。

## 环境依赖

- .NET SDK (>= 8.0)
- Visual Studio 或者 Visual Studio Code
- 支持 C# 的编辑器或 IDE

## 安装与配置

1. **克隆仓库**

   ```bash
   git clone <repository-url>
   cd <project-directory>
   ```

2. **安装 NuGet 包**

   使用 .NET CLI 安装所需的 NuGet 包：

   ```bash
   dotnet restore
   ```

3. **配置 JWT 设置**

   编辑 `appsettings.json` 文件以设置 JWT 参数，如密钥、发行人和受众等。

   ```json
   {
     "Jwt": {
       "Key": "your_secret_key_here",
       "Issuer": "Enrich",
       "Audience": "Client"
     }
   }
   ```

4. **运行应用程序**

   在项目根目录下执行以下命令启动应用：

   ```bash
   dotnet run
   ```

5. **访问 Swagger UI**

   启动后，可以通过浏览器访问 `https://localhost:<port>/swagger` 查看和测试 API。

## 使用说明

### 测试受保护的 API

一旦你有了 JWT 令牌，就可以将其放在 HTTP 请求头中的 `Authorization` 字段来访问受保护的 API 端点。例如：

```http
GET /api/v1/system/server_time HTTP/1.1
Host: localhost:5001
Authorization: Bearer <your_jwt_token_here>
```

### 测试无需授权的 API

某些 API 端点不需要 JWT 令牌即可访问，比如欢迎信息接口：

```http
GET /api/v1/system/wellcome HTTP/1.1
Host: localhost:5001
```

### 获取 JWT 令牌示例

```http
GET /api/v1/system/get_jwt HTTP/1.1
Host: localhost:5001
```

## 注意事项

- 确保不要将敏感信息（如私钥）提交到公共代码库中。
- 在生产环境中启用 HTTPS 并配置适当的 SSL/TLS 证书。
- 对于更复杂的应用场景，请考虑添加更多的安全措施，如刷新令牌机制、令牌黑名单等。

## 参考资料

- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [JSON Web Tokens](https://jwt.io/)
- [Knife4j Swagger](https://github.com/xiaoym/knife4j)

