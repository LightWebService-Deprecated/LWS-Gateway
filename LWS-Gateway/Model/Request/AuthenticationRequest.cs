using System.Diagnostics.CodeAnalysis;

namespace LWS_Gateway.Model.Request
{
    [ExcludeFromCodeCoverage]
    public class AuthenticationRequest
    {
        public string UserToken { get; set; }
    }
}