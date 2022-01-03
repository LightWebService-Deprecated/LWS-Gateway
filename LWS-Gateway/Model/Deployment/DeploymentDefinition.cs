using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LWS_Gateway.Model.Deployment;

[ExcludeFromCodeCoverage]
public class DeploymentDefinition
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string Id { get; set; }
    public string UserId { get; set; }
    public string ServiceName { get; set; }
    public string DeploymentName { get; set; }
    public DeploymentType DeploymentType { get; set; }
    public List<int> DeploymentOpenedPorts { get; set; }

    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}