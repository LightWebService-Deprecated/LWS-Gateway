using System.Collections.Generic;

namespace LWS_Gateway.Model.Deployment;

public class KubeUserDeployment
{
    public string UserId { get; set; }
    public List<DeploymentDefinition> DeploymentDefinitions { get; set; }
}
