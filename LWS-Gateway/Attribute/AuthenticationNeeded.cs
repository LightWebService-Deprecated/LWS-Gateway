using System.Diagnostics.CodeAnalysis;
using LWS_Gateway.Extension;
using LWS_Gateway.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LWS_Gateway.Attribute
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationNeeded: ActionFilterAttribute
    {
        public AccountRole TargetRole { get; set; } = AccountRole.User;
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            if (!httpContext.IsUserIdExists())
            {
                context.Result = new UnauthorizedObjectResult(
                    new
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        ErrorPath = context.HttpContext.Request.Path,
                        Message = "Access Denied",
                        DetailedMessage = "This API needs to be logged-in. Please login!"
                    });
            }
            else
            {
                var roleExists = httpContext.GetUserRole()?.Contains(TargetRole);
            
                if (roleExists is null or false)
                {
                    context.Result = new UnauthorizedObjectResult(
                        new
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            ErrorPath = context.HttpContext.Request.Path,
                            Message = "Access Denied",
                            DetailedMessage = $"This API needs {AccountRole.GetName(TargetRole)}!"
                        });
                }
            }
            
            base.OnActionExecuting(context);
        }
    }
}