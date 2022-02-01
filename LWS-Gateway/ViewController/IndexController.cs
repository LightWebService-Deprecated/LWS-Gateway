using System.Linq;
using System.Threading.Tasks;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.ViewController;

[Route("/")]
public class IndexController: Controller
{
    private readonly UserService _userService;
    private readonly AuthenticationService _authenticationService;
    
    public IndexController(UserService userService, AuthenticationService authenticationService)
    {
        _userService = userService;
        _authenticationService = authenticationService;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        // Get Token
        var token = HttpContext.User.Claims.FirstOrDefault(a => a.Type == "token");

        var accountInfo =
            await _authenticationService.AuthenticateUserRequest(new AuthenticationRequest {UserToken = token.Value});
        
        return View(accountInfo.ToViewHeaderResponse());
    }
}