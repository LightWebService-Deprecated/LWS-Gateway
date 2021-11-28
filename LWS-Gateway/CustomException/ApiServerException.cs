using System;
using System.Diagnostics.CodeAnalysis;

namespace LWS_Gateway.CustomException
{
    [ExcludeFromCodeCoverage]
    public class ApiServerException: System.Exception
    {
        public readonly int HttpStatusCode;
        public readonly string Message;

        public readonly Exception Exception;

        public ApiServerException(int statusCode, string message)
        {
            HttpStatusCode = statusCode;
            Message = message;
        }

        public ApiServerException(int statusCode, string message, System.Exception exception)
        {
            HttpStatusCode = statusCode;
            Message = message;
            Exception = exception;
        }
    }
}