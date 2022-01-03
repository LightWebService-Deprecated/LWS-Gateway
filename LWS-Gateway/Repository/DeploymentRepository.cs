using System.Collections.Generic;
using System.Threading.Tasks;
using LWS_Gateway.Model.Deployment;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace LWS_Gateway.Repository;

public interface IDeploymentRepository
{
    /// <summary>
    /// Create Deployment data object to mongoDB
    /// </summary>
    /// <param name="deploymentDefinition">Definition to save.</param>
    /// <returns>Nothing</returns>
    Task CreateDeployment(DeploymentDefinition deploymentDefinition);
    
    /// <summary>
    /// List all user deployment, in list.
    /// </summary>
    /// <param name="userId">target user.</param>
    /// <returns>List of deployment definition.</returns>
    Task<List<DeploymentDefinition>> ListUserDeployment(string userId);
}

public class DeploymentRepository: IDeploymentRepository
{
    private readonly IMongoCollection<DeploymentDefinition> _deploymentCollection;

    public DeploymentRepository(MongoContext mongoContext)
    {
        _deploymentCollection =
            mongoContext.MongoDatabase.GetCollection<DeploymentDefinition>(nameof(DeploymentDefinition));
    }

    public async Task CreateDeployment(DeploymentDefinition deploymentDefinition)
    {
        await _deploymentCollection.InsertOneAsync(deploymentDefinition);
    }

    public async Task<List<DeploymentDefinition>> ListUserDeployment(string userId)
    {
        return await _deploymentCollection.AsQueryable()
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }
}