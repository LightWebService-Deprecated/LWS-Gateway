using System.Diagnostics.CodeAnalysis;

namespace LWS_Gateway.Model.Request
{
    [ExcludeFromCodeCoverage]
    public class LoginRequest
    {
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
    }
}