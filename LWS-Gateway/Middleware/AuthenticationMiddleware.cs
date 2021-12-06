using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LWS_Gateway.Extension;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Http;

namespace LWS_Gateway.Middleware
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly UserService _userService;

        public AuthenticationMiddleware(RequestDelegate requestDelegate, UserService userService)
        {
            _requestDelegate = requestDelegate;
            _userService = userService;
        }

        public async Task Invoke(HttpContext context)
        {
            var userToken = context.Request.Headers["X-API-AUTH"].FirstOrDefault();

            if (userToken != null)
            {
                // Authenticate from Authentication Service
                var accountEntity =
                    await _userService.AuthenticateUserRequest(new AuthenticationRequest {UserToken = userToken});

                if (accountEntity != null)
                {
                    context.SetUserEmail(accountEntity.UserEmail);
                    context.SetUserRole(accountEntity.AccountRoles);
                }
            }
            
            await _requestDelegate(context);
        }
    }
}