using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LWS_Gateway.Model.Deployment;

[ExcludeFromCodeCoverage]
public class KubeUserDeployment
{
    public string UserId { get; set; }
    public List<DeploymentDefinition> DeploymentDefinitions { get; set; }
}
