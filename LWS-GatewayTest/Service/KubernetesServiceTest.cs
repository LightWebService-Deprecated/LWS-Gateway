using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allure.Xunit.Attributes;
using k8s;
using k8s.Models;
using LWS_Gateway.Kube;
using LWS_Gateway.Model.Deployment;
using LWS_Gateway.Service;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LWS_GatewayTest.Service;

[Collection("DockerIntegration")]
[AllureSuite("Kubernetes Site Integration Test")]
public class KubernetesServiceTest: IDisposable
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly IKubernetes _testClient;
    private readonly IKubernetesService _kubernetesService;

    public KubernetesServiceTest()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(a => a["KubePath"])
            .Returns("/tmp/kubeconfig.yaml");
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(_mockConfiguration.Object["KubePath"]);
        config.SkipTlsVerify = false;

        _testClient = new Kubernetes(config);
        _kubernetesService = new KubernetesService(_mockConfiguration.Object, new ServiceDeploymentProvider());
    }

    public void Dispose()
    {
        ResetKube().Wait();
    }

    private async Task ResetKube()
    {
        var namespaceBlackList = new List<string>
        {
            "default",
            "kube-system",
            "kube-public",
            "kube-node-lease"
        };
        
        var namespaceResponse = await _testClient.ListNamespaceWithHttpMessagesAsync();
        var namespaceToRemove = namespaceResponse.Body.Items
            .Where(a => !namespaceBlackList.Contains(a.Name()));
        foreach (var eachNamespace in namespaceToRemove)
        {
            await _testClient.DeleteNamespaceWithHttpMessagesAsync(eachNamespace.Name());
        }
    }

    private async Task EnsureNamespaceCreated(string namespaceName)
    {
        var namespaceResponse = await _testClient.ListNamespaceWithHttpMessagesAsync();
        var namespaceList = namespaceResponse.Body.Items.Select(a => a.Name());
        
       Assert.Contains(namespaceName, namespaceList);
    }

    private async Task EnsureNamespaceDeleted(string namespaceName)
    {
        var namespaceResponse = await _testClient.ListNamespaceWithHttpMessagesAsync();
        var namespaceList = namespaceResponse.Body.Items
            .Where(a => a.Status.Phase == "Active")
            .Select(a => a.Name());
        
        Assert.DoesNotContain(namespaceName, namespaceList);
    }

    private async Task EnsureDeploymentDeleted(string deploymentName)
    {
        var deploymentResponse = await _testClient.ListDeploymentForAllNamespacesAsync();
        Assert.DoesNotContain(deploymentName, deploymentResponse.Items.Select(a => a.Name()));
    }

    private async Task EnsureDeploymentCreated(string deploymentName)
    {
        var deploymentResponse = await _testClient.ListDeploymentForAllNamespacesAsync();
        Assert.Contains(deploymentName, deploymentResponse.Items.Select(a => a.Name()));
    }
    
    private string GenerateRandomToken(int length = 64)
    {
        var random = new Random();
        var charDictionary = "1234567890abcdefghijklmnopqrstuvwxyz";

        return new string(Enumerable.Repeat(charDictionary, length)
            .Select(a => a[random.Next(charDictionary.Length)]).ToArray());
    }

    [AllureSubSuite("Namespace Creation Test")]
    [AllureXunitTheory(DisplayName = "CreateNameSpace: CreateNameSpace should create namespace with given name.")]
    [InlineData("testid")]
    [InlineData("testtwo")]
    [InlineData("somestrangeone")]
    public async void Is_CreateNameSpace_Creates_Namespace_Given_Name(string namespaceName)
    {
        // Do
        await _kubernetesService.CreateNameSpace(namespaceName);
        
        // Ensure
        await EnsureNamespaceCreated(namespaceName);
    }

    [AllureSubSuite("Namespace Deletion Test")]
    [AllureXunitTheory(DisplayName = "DeleteNameSpace: DeleteNameSpace should remove namespace with given name.")]
    [InlineData("testidwhat")]
    [InlineData("testidtwof")]
    [InlineData("strangename")]
    public async void Is_DeleteNameSpace_Removes_Name_Well(string namespaceName)
    {
        // Let
        await _kubernetesService.CreateNameSpace(namespaceName);
        await EnsureNamespaceCreated(namespaceName);
        
        // Do
        await _kubernetesService.DeleteNameSpace(namespaceName);
        
        // Check
        await EnsureNamespaceDeleted(namespaceName);
    }

    [AllureSubSuite("Deployment Creation Test")]
    [AllureXunit(DisplayName = "CreateDeployment: CreateDeployment should create ubuntu deployment well.")]
    public async void Is_CreateDeployment_Creates_Ubuntu_Well()
    {
        // Let
        var userId = GenerateRandomToken(10);
        await _kubernetesService.CreateNameSpace(userId);
        await EnsureNamespaceCreated(userId);
        
        // Do
        var deploymentDefinition = await _kubernetesService.CreateDeployment(userId, DeploymentType.Ubuntu);
        
        // Check
        Assert.NotNull(deploymentDefinition);
        Assert.Contains($"{userId.ToLower()}-ubuntu-", deploymentDefinition.DeploymentName);
        Assert.Equal(DeploymentType.Ubuntu, deploymentDefinition.DeploymentType);
        Assert.Contains(22, deploymentDefinition.DeploymentOpenedPorts);
        await EnsureDeploymentCreated(deploymentDefinition.DeploymentName);
    }

    [AllureSubSuite("Deployment Deletion Test")]
    [AllureXunit(DisplayName = "DeleteDeployment: DeleteDeployment should remove deployment well.")]
    public async void Is_DeleteDeployment_Deletes_Deployment_Well()
    {
        // Let
        var userId = GenerateRandomToken(10);
        await _kubernetesService.CreateNameSpace(userId);
        await EnsureNamespaceCreated(userId);
        var definition = await _kubernetesService.CreateDeployment(userId, DeploymentType.Ubuntu);
        await EnsureDeploymentCreated(definition.DeploymentName);
        
        // Do
        await _kubernetesService.DeleteDeployment(userId, definition.DeploymentName);
        
        // Check
        await EnsureDeploymentDeleted(definition.DeploymentName);
    }
}