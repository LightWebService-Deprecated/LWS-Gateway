using System.Diagnostics.CodeAnalysis;

namespace LWS_Gateway.Model.Request
{
    [ExcludeFromCodeCoverage]
    public class RegisterRequest
    {
        public string UserEmail { get; set; }
        public string UserNickName { get; set; }
        public string UserPassword { get; set; }
    }
}