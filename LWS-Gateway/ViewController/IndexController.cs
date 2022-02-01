using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.ViewController;

[Route("/")]
public class IndexController: Controller
{
    [HttpGet]
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }
}