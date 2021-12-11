using System.Threading.Tasks;
using LWS_Gateway.Model.Deployment;

namespace LWS_Gateway.Kube;

public interface IServiceDeployment
{
    public Task<DeploymentDefinition> CreateDeployment(string userId);
    public Task RemoveDeploymentByName(string userId, string deploymentId);
}