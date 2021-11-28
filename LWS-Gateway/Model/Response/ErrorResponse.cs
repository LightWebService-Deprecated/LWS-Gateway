namespace LWS_Gateway.Model.Response
{
    public class ErrorResponse
    {
        public int StatusCodes { get; set; }
        public string ErrorPath { get; set; }
        public string Message { get; set; }
        public string DetailedMessage { get; set; }
    }
}