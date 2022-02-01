using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace LWS_Gateway.Model;

[ExcludeFromCodeCoverage]
public class AccessToken
{
    [BsonElement("createdAt")]
    public long CreatedAt { get; set; }
    [BsonElement("expiresAt")]
    public long ExpiresAt { get; set; }
    [BsonElement("token")]
    public string Token { get; set; }
    
    [BsonElement("isExpiredExternally")]
    [JsonIgnore]
    public bool IsExpiredExternally { get; set; } = false;
}