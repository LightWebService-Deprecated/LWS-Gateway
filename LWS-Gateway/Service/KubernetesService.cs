using System.Collections.Generic;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using LWS_Gateway.Kube;
using LWS_Gateway.Model.Deployment;
using Microsoft.Extensions.Configuration;

namespace LWS_Gateway.Service;

public interface IKubernetesService
{
    Task CreateNameSpace(string userId);
    Task DeleteNameSpace(string userId);
    Task<DeploymentDefinition> CreateDeployment(string userId, DeploymentType deploymentType);
    Task SetupInitialVolume(string userId);
    Task DeleteDeployment(string userId, string deploymentName);
}

public class KubernetesService : IKubernetesService
{
    private readonly IKubernetes _client;
    private readonly ServiceDeploymentProvider _serviceDeploymentProvider;
    private readonly string _volumeNfsPath;
    private readonly string _volumeNfsServerAddr;

    public KubernetesService(IConfiguration configuration, ServiceDeploymentProvider deploymentProvider)
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configuration["KubePath"]);
        _client = new Kubernetes(config);

        _serviceDeploymentProvider = deploymentProvider;
        _volumeNfsServerAddr = configuration["KubeVolumeServer"];
        _volumeNfsPath = configuration["KubeVolumePath"];
    }

    public async Task CreateNameSpace(string userId)
    {
        var body = new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = userId
            }
        };

        await _client.CreateNamespaceWithHttpMessagesAsync(body);
    }

    public async Task DeleteNameSpace(string userId)
    {
        await _client.DeleteNamespaceWithHttpMessagesAsync(userId);
    }

    public async Task SetupInitialVolume(string userId)
    {
        await _client.CreatePersistentVolumeWithHttpMessagesAsync(new V1PersistentVolume
        {
            ApiVersion = "v1",
            Kind = "PersistentVolume",
            Metadata = new V1ObjectMeta
            {
                Name = $"{userId}-volume",
                Labels = new Dictionary<string, string>
                {
                    ["region"] = $"global-{userId}"
                }
            },
            Spec = new V1PersistentVolumeSpec
            {
                AccessModes = new List<string> {"ReadWriteMany"},
                Capacity = new Dictionary<string, ResourceQuantity>
                {
                    ["storage"] = new("256Mi")
                },
                Nfs = new V1NFSVolumeSource
                {
                    Server = _volumeNfsServerAddr,
                    Path = _volumeNfsPath
                }
            }
        });
    }

    public async Task<DeploymentDefinition> CreateDeployment(string userId, DeploymentType deploymentType)
    { 
        var serviceDeployment = _serviceDeploymentProvider.SelectCorrectDeployment(deploymentType, _client);
        var definition = await serviceDeployment.CreateDeployment(userId);
        
        // TODO: Save definition to user-deployment database.

        return definition;
    }

    public async Task DeleteDeployment(string userId, string deploymentName)
    {
        var serviceDeployment = _serviceDeploymentProvider.CreateUbuntuDeployment(_client);
        await serviceDeployment.RemoveDeploymentByName(userId, deploymentName);
    }
}