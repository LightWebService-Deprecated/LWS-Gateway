using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.Controllers;

[ApiController]
[Route("/api/misc")]
public class MiscController: ControllerBase
{
    [HttpGet("init")]
    public IActionResult Init()
    {
        return Ok();
    }
}