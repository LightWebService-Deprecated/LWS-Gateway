using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Net.Client;
using LWS_Authentication;
using LWS_Gateway.Attribute;
using LWS_Gateway.Model;
using LWS_Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LWS_Gateway.Controllers
{
    [ApiController]
    [Route("/api/user")]
    public class AuthController: SuperControllerBase
    {
        private readonly AuthenticationRpc.AuthenticationRpcClient _client;

        public AuthController(IConfiguration configuration)
        {
            var grpcChannel = GrpcChannel.ForAddress(configuration.GetConnectionString("AuthenticationServer"));
            _client = new AuthenticationRpc.AuthenticationRpcClient(grpcChannel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterRequestMessage registerMessage)
        {
            var result = await _client.RegisterRequestAsync(registerMessage);

            var handledCase = new Dictionary<ResultCode, Func<IActionResult>>()
            {
                [ResultCode.Success] = Ok,
                [ResultCode.Duplicate] = Conflict
            };

            return HandleCase(handledCase, result.ResultCode);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginRequestMessage loginMessage)
        {
            var result = await _client.LoginRequestAsync(loginMessage);

            var handledCase = new Dictionary<ResultCode, Func<IActionResult>>()
            {
                [ResultCode.Forbidden] = () => new StatusCodeResult(StatusCodes.Status403Forbidden),
                [ResultCode.Success] = () => Ok(JsonConvert.DeserializeObject<AccessToken>(result.Content))
            };

            return HandleCase(handledCase, result.ResultCode);
        }

        [HttpDelete]
        [AuthenticationNeeded]
        public async Task<IActionResult> DropoutUser()
        {
            var dropoutUserMessage = new DropoutUserMessage
            {
                UserEmail = HttpContext.Items["userEmail"]!.ToString()
            };
            
            var result = await _client.DropoutUserRequestAsync(dropoutUserMessage);

            var handledCase = new Dictionary<ResultCode, Func<IActionResult>>()
            {
                [ResultCode.Success] = Ok
            };

            return HandleCase(handledCase, result.ResultCode);
        }

        [HttpPost("auth")]
        public async Task<IActionResult> AuthenticateUserAsync(AuthenticateUserMessage authMessage)
        {
            var result = await _client.AuthenticateUserRequestAsync(authMessage);

            var handledCase = new Dictionary<ResultCode, Func<IActionResult>>()
            {
                [ResultCode.Success] = Ok,
                [ResultCode.Forbidden] = () => new StatusCodeResult(StatusCodes.Status403Forbidden)
            };

            return HandleCase(handledCase, result.ResultCode);   
        }
    }
}