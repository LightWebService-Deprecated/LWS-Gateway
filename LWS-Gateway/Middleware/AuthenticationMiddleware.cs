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
        private readonly AuthenticationService _authenticationService;

        public AuthenticationMiddleware(RequestDelegate requestDelegate, AuthenticationService authentication)
        {
            _requestDelegate = requestDelegate;
            _authenticationService = authentication;
        }

        public async Task Invoke(HttpContext context)
        {
            var userToken = context.Request.Headers["X-API-AUTH"].FirstOrDefault();

            if (userToken != null)
            {
                // Authenticate from Authentication Service
                var accountEntity =
                    await _authenticationService.AuthenticateUserRequest(new AuthenticationRequest {UserToken = userToken});

                if (accountEntity != null)
                {
                    context.SetUserId(accountEntity.Id);
                    context.SetUserRole(accountEntity.AccountRoles);
                }
            }
            
            await _requestDelegate(context);
        }
    }
}