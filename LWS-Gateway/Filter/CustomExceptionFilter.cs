using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LWS_Gateway.CustomException;
using LWS_Gateway.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Rest;

namespace LWS_Gateway.Filter
{
    public class RawJsonToObjectResult : IActionResult
    {
        public string JsonString { get; set; }
        public int StatusCode { get; set; }
        
        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCode;
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            await using var streamWriter = new StreamWriter(context.HttpContext.Response.Body, Encoding.UTF8);
            await streamWriter.WriteAsync(JsonString);
            await streamWriter.FlushAsync();
        }
    }
    
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
            else if (context.Exception is HttpOperationException)
            {
                var correctException = context.Exception as HttpOperationException;
                context.Result = new RawJsonToObjectResult
                {
                    JsonString = correctException.Response.Content,
                    StatusCode = (int)correctException.Response.StatusCode
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