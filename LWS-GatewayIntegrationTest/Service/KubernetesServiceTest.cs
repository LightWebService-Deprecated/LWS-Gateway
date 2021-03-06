using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using LWS_Gateway.Configuration;
using LWS_Gateway.Kube;
using LWS_Gateway.Model.Deployment;
using LWS_Gateway.Repository;
using LWS_Gateway.Service;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace LWS_GatewayIntegrationTest.Service;

[Collection("DockerIntegration")]
public class KubernetesServiceTest: IDisposable
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly IKubernetes _testClient;
    private readonly IKubernetesService _kubernetesService;
    private readonly IMongoCollection<DeploymentDefinition> _deploymentCollection;

    public KubernetesServiceTest()
    {
        var mongoContext = new MongoContext(new MongoConfiguration
        {
            MongoConnection = "mongodb://root:testPassword@localhost:27017",
            MongoDbName = Guid.NewGuid().ToString()
        });
        
        _deploymentCollection =
            mongoContext.MongoDatabase.GetCollection<DeploymentDefinition>(nameof(DeploymentDefinition));
        
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(a => a["KubePath"])
            .Returns("/tmp/kubeconfig.yaml");
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(_mockConfiguration.Object["KubePath"]);
        config.SkipTlsVerify = false;

        _testClient = new Kubernetes(config);
        _kubernetesService = new KubernetesService(_mockConfiguration.Object, new ServiceDeploymentProvider(), new DeploymentRepository(mongoContext));
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

    [Theory(DisplayName = "CreateNameSpace: CreateNameSpace should create namespace with given name.")]
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

    [Theory(DisplayName = "DeleteNameSpace: DeleteNameSpace should remove namespace with given name.")]
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

    [Fact(DisplayName = "CreateDeployment: CreateDeployment should create ubuntu deployment well.")]
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
        
        // Check MongoDb
        var list = await _deploymentCollection.AsQueryable().ToListAsync();
        Assert.Single(list);
        Assert.Equal(userId, list[0].UserId);

        // Check service object created
        var serviceList = await _testClient.ListNamespacedServiceWithHttpMessagesAsync(userId);
        Assert.NotNull(serviceList);
        Assert.True(serviceList.Body.Items.Count >= 1);
        var ubuntuService = serviceList.Body.Items
            .FirstOrDefault(a => a.Metadata.Name.Contains("ubuntu-service"));
        Assert.NotNull(ubuntuService);
    }

    [Fact(DisplayName = "DeleteDeployment: DeleteDeployment should remove deployment well.")]
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

    [Fact(DisplayName = "GetUserDeployment: GetUserDeployment should return list of deployments well.")]
    public async void Is_GetUserDeployment_Creates_Deployment_Well()
    {
        // Let
        var testId = "testId";
        var definition = new DeploymentDefinition
        {
            UserId = testId,
            DeploymentName = "TestName"
        };
        await _deploymentCollection.InsertOneAsync(definition);
        
        // Do
        var list = await _deploymentCollection.AsQueryable().ToListAsync();
        
        // Check
        Assert.Single(list);
        Assert.Equal(testId, list[0].UserId);
        Assert.Equal(definition.DeploymentName, list[0].DeploymentName);
    }
}