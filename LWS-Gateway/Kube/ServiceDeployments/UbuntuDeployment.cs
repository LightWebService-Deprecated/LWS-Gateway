using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using LWS_Gateway.Model.Deployment;

namespace LWS_Gateway.Kube.ServiceDeployments;

public class UbuntuDeployment: IServiceDeployment
{
    private const int SshPort = 22;
    private readonly IKubernetes _kubernetesClient;

    public UbuntuDeployment(IKubernetes kubernetes)
    {
        _kubernetesClient = kubernetes;
    }
    
    private V1Container DefaultUbuntuContainer => new()
    {
        Image = "kangdroid/multiarch-sshd",
        Name = $"ubuntu-sshd-kdr-{Guid.NewGuid().ToString()}",
        Ports = new List<V1ContainerPort>
        {
            new(containerPort: SshPort, protocol: "TCP")
        },
        VolumeMounts = new List<V1VolumeMount>
        {
            new()
            {
                MountPath = "/test",
                Name = "volume-test"
            }
        }
    };

    private V1Deployment CreateUbuntuDeploymentObject(string userId)
    {
        var deploymentLabel = $"{userId}.deployment.ubuntu.{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var deploymentPodLabel = new Dictionary<string, string>
        {
            ["name"] = $"{userId}.ubuntu.pods-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };
        
        return new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Labels = new Dictionary<string, string>
                {
                    ["deploymentIdentifier"] = deploymentLabel
                },
                Name = $"{userId.ToLower()}-ubuntu-{GenerateRandomToken(5)}"
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
                        Containers = new List<V1Container> { DefaultUbuntuContainer },
                        Volumes = new List<V1Volume>
                        {
                            new()
                            {
                                Name = "volume-test",
                                PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource()
                                {
                                    ClaimName = "global-volume-claim"
                                }
                            }
                        }
                    }
                }
            }
        };
    }
    
    private string GenerateRandomToken(int length = 64)
    {
        var random = new Random();
        var charDictionary = "1234567890abcdefghijklmnopqrstuvwxyz";

        return new string(Enumerable.Repeat(charDictionary, length)
            .Select(a => a[random.Next(charDictionary.Length)]).ToArray());
    }
    
    public async Task<DeploymentDefinition> CreateDeployment(string userId)
    {
        var deployment = CreateUbuntuDeploymentObject(userId);
        await _kubernetesClient.CreateNamespacedDeploymentWithHttpMessagesAsync(deployment, userId);
        
        return new DeploymentDefinition
        {
            DeploymentName = deployment.Metadata.Name,
            DeploymentType = DeploymentType.Ubuntu,
            DeploymentOpenedPorts = new List<int> {SshPort}
        };
    }

    public async Task RemoveDeploymentByName(string userId, string deploymentName)
    {
        await _kubernetesClient.DeleteNamespacedDeploymentAsync(deploymentName, userId);
    }
}