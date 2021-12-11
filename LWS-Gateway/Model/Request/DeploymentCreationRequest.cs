using System.Diagnostics.CodeAnalysis;
using LWS_Gateway.Model.Deployment;

namespace LWS_Gateway.Model.Request;

[ExcludeFromCodeCoverage]
public class DeploymentCreationRequest
{
    public DeploymentType DeploymentType { get; set; }
}