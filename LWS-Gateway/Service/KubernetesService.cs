using System;
using System.Collections.Generic;
using k8s;
using k8s.Models;

namespace LWS_Gateway.Service;

public class KubernetesService
{
    private readonly IKubernetes _client;

    public KubernetesService()
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        _client = new Kubernetes(config);
    }

    private V1Container UbuntuSshdContainer => new()
    {
        Image = "kangdroid/multiarch-sshd",
        Name = $"UBUNTU_SSHD_KDR/{Guid.NewGuid().ToString()}",
        Ports = new List<V1ContainerPort>
        {
            new(containerPort: 22, protocol: "TCP")
        }
    };

    public void CreateUbuntuDeployment(string userId)
    {
        var deploymentLabel = $"{userId}/deployment/ubuntu/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var deploymentPodLabel = new Dictionary<string, string>
        {
            ["name"] = $"{userId}/ubuntu/pods-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };
        
        var deployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Labels = new Dictionary<string, string>
                {
                    ["deploymentIdentifier"] = deploymentLabel
                }
            },
            Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector { MatchLabels = deploymentPodLabel },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = deploymentPodLabel },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container> { UbuntuSshdContainer }
                    }
                }
            }
        };
    }
}