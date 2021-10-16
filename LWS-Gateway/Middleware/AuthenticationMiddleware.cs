using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Google.Api;
using Grpc.Net.Client;
using LWS_Authentication;
using LWS_Gateway.Model;
using LWS_Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LWS_Gateway.Middleware
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly AuthenticationRpc.AuthenticationRpcClient _authenticationClient;

        public AuthenticationMiddleware(RequestDelegate requestDelegate, IConfiguration configuration)
        {
            _requestDelegate = requestDelegate;
            var grpcChannel = GrpcChannel.ForAddress(configuration.GetConnectionString("AuthenticationServer"));
            _authenticationClient = new AuthenticationRpc.AuthenticationRpcClient(grpcChannel);
        }

        public async Task Invoke(HttpContext context)
        {
            var userToken = context.Request.Headers["X-API-AUTH"].FirstOrDefault();

            if (userToken != null)
            {
                // Authenticate from Authentication Service
                var result = await _authenticationClient.AuthenticateUserRequestAsync(new AuthenticateUserMessage
                    {UserToken = userToken});
            
                // If result is success
                if (result?.ResultCode == ResultCode.Success)
                {
                    var accountInfo = JsonConvert.DeserializeObject<AccountProjection>(result.Content);
                    if (accountInfo != null) context.Items["userEmail"] = accountInfo.UserEmail;
                }
            }
            
            await _requestDelegate(context);
        }
    }
}