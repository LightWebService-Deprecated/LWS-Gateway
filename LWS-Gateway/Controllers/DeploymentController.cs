using System.Threading.Tasks;
using LWS_Gateway.Attribute;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.Controllers;

[ApiController]
[Route("/api/v1/deployment")]
public class DeploymentController: ControllerBase
{
    private readonly KubernetesService _kubernetesService;

    public DeploymentController(KubernetesService kubernetesService)
    {
        _kubernetesService = kubernetesService;
    }

    [HttpPost]
    [AuthenticationNeeded(TargetRole = AccountRole.User)]
    public async Task<IActionResult> CreateDeployment(DeploymentCreationRequest request)
    {
        var userId = "testid";
        var definition = await _kubernetesService.CreateDeployment(userId, request.DeploymentType);

        return Ok(definition);
    }

    [HttpDelete]
    [AuthenticationNeeded(TargetRole = AccountRole.User)]
    public async Task<IActionResult> DeleteDeployment(DeploymentDeleteRequest deleteRequest)
    {
        var userId = "testid";
        await _kubernetesService.DeleteDeployment(userId, deleteRequest.DeploymentName);

        return Ok();
    }
}