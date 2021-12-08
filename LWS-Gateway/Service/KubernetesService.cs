using System.Threading.Tasks;
using k8s;
using k8s.Models;
using LWS_Gateway.Kube;
using LWS_Gateway.Model.Deployment;
using Microsoft.Extensions.Configuration;

namespace LWS_Gateway.Service;

public class KubernetesService
{
    private readonly IKubernetes _client;
    private readonly ServiceDeploymentProvider _serviceDeploymentProvider;

    public KubernetesService(IConfiguration configuration, ServiceDeploymentProvider deploymentProvider)
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configuration["KubePath"]);
        _client = new Kubernetes(config);

        _serviceDeploymentProvider = deploymentProvider;
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