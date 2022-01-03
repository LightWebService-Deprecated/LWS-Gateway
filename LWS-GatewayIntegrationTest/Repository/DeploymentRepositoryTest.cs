using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LWS_Gateway.Configuration;
using LWS_Gateway.Model.Deployment;
using LWS_Gateway.Repository;
using MongoDB.Driver;
using Xunit;

namespace LWS_GatewayIntegrationTest.Repository;

[Collection("DockerIntegration")]
public class DeploymentRepositoryTest
{
    private readonly IDeploymentRepository _deploymentRepository;
    private readonly IMongoCollection<DeploymentDefinition> _deploymentCollection;

    public DeploymentRepositoryTest()
    {
        var mongoContext = new MongoContext(new MongoConfiguration
        {
            MongoConnection = "mongodb://root:testPassword@localhost:27017",
            MongoDbName = $"{Guid.NewGuid().ToString()}"
        });

        _deploymentCollection =
            mongoContext.MongoDatabase.GetCollection<DeploymentDefinition>(nameof(DeploymentDefinition));
        _deploymentRepository = new DeploymentRepository(mongoContext);
    }

    private async Task<List<DeploymentDefinition>> GetDeploymentDefinition()
    {
        return await _deploymentCollection.AsQueryable()
        .ToListAsync();
    }

    [Fact(DisplayName = "CreateDeployment: CreateDeployment should save deployment definition to database.")]
    public async Task Is_CreateDeployment_Saves_Well()
    {
        // Let
        var definition = new DeploymentDefinition
        {
            DeploymentName = "TestName"
        };
        
        // Do
        await _deploymentRepository.CreateDeployment(definition);
        
        // Check
        var definitionList = await GetDeploymentDefinition();
        Assert.Single(definitionList);
        Assert.Equal(definition.DeploymentName, definitionList[0].DeploymentName);
    }

    [Fact(DisplayName = "ListUserDeployment: ListUserDeployment should list all deployment available.")]
    public async Task Is_ListUserDeployment_Lists_All_Deployment()
    {
        // Let
        var testId = "testId";
        var definition = new DeploymentDefinition
        {
            UserId = testId,
            DeploymentName = "TestName"
        };
        await _deploymentRepository.CreateDeployment(definition);
        
        // Do
        var list = await _deploymentRepository.ListUserDeployment(testId);
        
        // Check
        Assert.Single(list);
        Assert.Equal(testId, list[0].UserId);
        Assert.Equal(definition.DeploymentName, list[0].DeploymentName);
    }
}