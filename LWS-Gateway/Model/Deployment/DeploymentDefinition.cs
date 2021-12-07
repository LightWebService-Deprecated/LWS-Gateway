using System;
using System.Collections.Generic;

namespace LWS_Gateway.Model.Deployment;

public class DeploymentDefinition
{
    public string DeploymentName { get; set; }
    public DeploymentType DeploymentType { get; set; }
    public List<int> DeploymentOpenedPorts { get; set; }

    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}