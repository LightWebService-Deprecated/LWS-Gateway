using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LWS_Gateway.Attribute
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationNeeded: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            if (!httpContext.Items.ContainsKey("userEmail"))
                context.Result = new UnauthorizedObjectResult(
                    new
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        ErrorPath = context.HttpContext.Request.Path,
                        Message = "Access Denied",
                        DetailedMessage = "This API needs to be logged-in. Please login!"
                    });
            base.OnActionExecuting(context);
        }
    }
}