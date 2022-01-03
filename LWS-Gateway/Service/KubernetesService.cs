using System.Collections.Generic;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using LWS_Gateway.Kube;
using LWS_Gateway.Model.Deployment;
using LWS_Gateway.Repository;
using Microsoft.Extensions.Configuration;

namespace LWS_Gateway.Service;

public interface IKubernetesService
{
    Task CreateNameSpace(string userId);
    Task DeleteNameSpace(string userId);
    Task<DeploymentDefinition> CreateDeployment(string userId, DeploymentType deploymentType);
    Task DeleteDeployment(string userId, string deploymentName);
    Task<List<DeploymentDefinition>> GetUserDeployment(string userId);
}

public class KubernetesService : IKubernetesService
{
    private readonly IKubernetes _client;
    private readonly ServiceDeploymentProvider _serviceDeploymentProvider;
    private readonly IDeploymentRepository _deploymentRepository;

    public KubernetesService(IConfiguration configuration, ServiceDeploymentProvider deploymentProvider, DeploymentRepository deploymentRepository)
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configuration["KubePath"]);
        _client = new Kubernetes(config);

        _serviceDeploymentProvider = deploymentProvider;
        _deploymentRepository = deploymentRepository;
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
        await _deploymentRepository.CreateDeployment(definition);

        return definition;
    }

    public async Task<List<DeploymentDefinition>> GetUserDeployment(string userId)
    {
        return await _deploymentRepository.ListUserDeployment(userId);
    }

    public async Task DeleteDeployment(string userId, string deploymentName)
    {
        var serviceDeployment = _serviceDeploymentProvider.CreateUbuntuDeployment(_client);
        await serviceDeployment.RemoveDeploymentByName(userId, deploymentName);
    }
}