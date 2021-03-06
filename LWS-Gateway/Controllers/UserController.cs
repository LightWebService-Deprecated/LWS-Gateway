using System.Threading.Tasks;
using LWS_Gateway.Attribute;
using LWS_Gateway.Extension;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.Controllers
{
    [ApiController]
    [Route("/api/user")]
    public class UserController: ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [AuthenticationNeeded(TargetRole = AccountRole.User)]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            return Ok(await _userService.GetAccountProjection(HttpContext.GetUserId()));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterRequest registerMessage)
        {
            await _userService.RegisterRequest(registerMessage);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginRequest loginMessage)
        {
            return Ok(await _userService.LoginRequest(loginMessage));
        }

        [HttpDelete]
        [AuthenticationNeeded(TargetRole = AccountRole.User)]
        public async Task<IActionResult> DropoutUser()
        {
            await _userService.DropoutUserRequest(HttpContext.GetUserId());

            return Ok();
        }
    }
}