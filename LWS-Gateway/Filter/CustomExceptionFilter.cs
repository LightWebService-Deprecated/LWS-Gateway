using System.Diagnostics.CodeAnalysis;
using LWS_Gateway.CustomException;
using LWS_Gateway.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LWS_Gateway.Filter
{
    [ExcludeFromCodeCoverage]
    public class CustomExceptionFilter: IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ApiServerException exception)
            {
                var errorResponse = HandleApiServerException(exception,
                    context.HttpContext.Request.Path.Value);
                context.Result = new ObjectResult(errorResponse)
                {
                    StatusCode = errorResponse.StatusCodes
                };
            }
            else
            {
                var errorResponse = new ErrorResponse
                {
                    StatusCodes = StatusCodes.Status500InternalServerError,
                    Message = "Unknown Error Occurred!",
                    DetailedMessage = context.Exception.StackTrace,
                    ErrorPath = context.HttpContext.Request.Path.Value
                };
                context.Result = new ObjectResult(errorResponse)
                {
                    StatusCode = errorResponse.StatusCodes
                };
            }
        }

        private ErrorResponse HandleApiServerException(ApiServerException exception, string path)
        {
            return new ErrorResponse
            {
                Message = exception.Message,
                DetailedMessage = exception.StackTrace,
                StatusCodes = exception.HttpStatusCode,
                ErrorPath = path
            };
        }
    }
}