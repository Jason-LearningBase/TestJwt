using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestJwt.Tools;

namespace TestJwt.Controllers
{
    /// <summary>
    /// ϵͳ������
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
        /// ϵͳ��ӭ��Ϣ(����������֤)
        /// </summary>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        [Route("wellcome")]
        public IActionResult GetWellcomeMsg()
        {
            // ���ؽṹ����JSON���
            return new JsonResult(new { code = 0, status = true, message = "���ã���ӭʹ��Sinno-MCM", data = new { } });
        }


        /// <summary>
        /// ��ȡ������ʱ��get
        /// </summary>
        /// <returns>��������ǰʱ��</returns>
        [HttpGet("server_time")]
        public IActionResult GetServerTime()
        {
            // ���ؽṹ����JSON���
            return new JsonResult(new { code = 0, status = true, message = "OK", data = new { serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") } });
        }

        /// <summary>
        /// ��ȡ������ʱ��post
        /// </summary>
        /// <returns>��������ǰʱ��</returns>
        [HttpPost("server_time2")]
        public IActionResult GetServerTime2()
        {
            // ���ؽṹ����JSON���
            return new JsonResult(new { code = 0, status = true, message = "OK", data = new { serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") } });
        }


        /// <summary>
        /// ��ȡһ��Jwt
        /// </summary>
        /// <returns>��ǰ�û���Ϣ</returns>
        [HttpGet("get_jwt")]
        [AllowAnonymous]
        public IActionResult GetJwt()
        {
            // ��ȡ��ǰ�û���Ϣ
            var userID = Guid.Parse("d0f0c0b0-a0b0-c0d0-e0f0-000000000000");
            var loginPlatform = "PC";

            // ����Jwt
            var jwtToken = "Bearer " +  _jwtTokenProvider.GenerateToken(userID, loginPlatform, 5);

            // ���ؽṹ����JSON���
            return new JsonResult(new { code = 0, status = true, message = "OK", data = new { jwtToken } });

        }

    }

}
