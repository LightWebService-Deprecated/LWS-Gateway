using System.Diagnostics.CodeAnalysis;

namespace LWS_Gateway.Model.Request;

[ExcludeFromCodeCoverage]
public class DeploymentDeleteRequest
{
    public string DeploymentName { get; set; }
}