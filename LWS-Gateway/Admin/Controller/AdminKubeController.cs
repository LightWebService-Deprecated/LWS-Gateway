using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LWS_Gateway.Admin.Controller;

[ExcludeFromCodeCoverage]
[ApiController]
[Route("/api/admin/cluster")]
public class AdminKubeController: ControllerBase
{
    private readonly IKubernetesService _kubernetesService;
    private readonly ILogger _logger;

    public AdminKubeController(IKubernetesService kubernetesService, ILogger<AdminKubeController> logger)
    {
        _kubernetesService = kubernetesService;
        _logger = logger;
    }
    
    [HttpDelete]
    public async Task<IActionResult> RemoveUserClusterAsync(string userId)
    {
        _logger.LogInformation("Removing cluster for user: {userId}", userId);
        await _kubernetesService.DeleteNameSpace(userId);
        return Ok();
    }
}