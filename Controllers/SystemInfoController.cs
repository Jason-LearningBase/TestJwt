using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestJwt.Tools;

namespace TestJwt.Controllers
{
    /// <summary>
    /// 系统控制器
    /// </summary>
    [ApiController]
    [Route("api/v1/system/[controller]"), ApiExplorerSettings(GroupName = "System")]
    [Authorize]
    public class SystemInfoController : Controller
    {
        private readonly ILogger<SystemInfoController> _logger;
        private readonly IConfiguration _configuration;
        private readonly JwtTokenProvider _jwtTokenProvider;


        public SystemInfoController(ILogger<SystemInfoController> logger, IConfiguration configuration, JwtTokenProvider jwtTokenProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _jwtTokenProvider = jwtTokenProvider;
        }

        /// <summary>
        /// 系统欢迎信息(匿名无需认证)
        /// </summary>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        [Route("wellcome")]
        public IActionResult GetWellcomeMsg()
        {
            // 返回结构化的JSON结果
            return new JsonResult(new { code = 0, status = true, message = "您好！欢迎使用Sinno-MCM", data = new { } });
        }


        /// <summary>
        /// 获取服务器时间get
        /// </summary>
        /// <returns>服务器当前时间</returns>
        [HttpGet("server_time")]
        public IActionResult GetServerTime()
        {
            // 返回结构化的JSON结果
            return new JsonResult(new { code = 0, status = true, message = "OK", data = new { serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") } });
        }

        /// <summary>
        /// 获取服务器时间post
        /// </summary>
        /// <returns>服务器当前时间</returns>
        [HttpPost("server_time2")]
        public IActionResult GetServerTime2()
        {
            // 返回结构化的JSON结果
            return new JsonResult(new { code = 0, status = true, message = "OK", data = new { serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") } });
        }


        /// <summary>
        /// 获取一个Jwt
        /// </summary>
        /// <returns>当前用户信息</returns>
        [HttpGet("get_jwt")]
        [AllowAnonymous]
        public IActionResult GetJwt()
        {
            // 获取当前用户信息
            var userID = Guid.Parse("d0f0c0b0-a0b0-c0d0-e0f0-000000000000");
            var loginPlatform = "PC";

            // 生成Jwt
            var jwtToken = "Bearer " +  _jwtTokenProvider.GenerateToken(userID, loginPlatform, 5);

            // 返回结构化的JSON结果
            return new JsonResult(new { code = 0, status = true, message = "OK", data = new { jwtToken } });

        }

    }

}
