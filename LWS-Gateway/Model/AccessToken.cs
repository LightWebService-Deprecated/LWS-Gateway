using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace LWS_Gateway.Model;

[ExcludeFromCodeCoverage]
public class AccessToken
{
    public long CreatedAt { get; set; }
    public long ExpiresAt { get; set; }
    public string Token { get; set; }
    
    [JsonIgnore]
    public bool IsExpiredExternally { get; set; } = false;
}