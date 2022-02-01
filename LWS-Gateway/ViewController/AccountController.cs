using LWS_Gateway.Service;
using Microsoft.AspNetCore.Mvc;

namespace LWS_Gateway.ViewController;

[Route("/account")]
public class AccountController: Controller
{
    private readonly UserService _userService;
    
    public AccountController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("login")]
    public IActionResult LoginUser()
    {
        return View();
    }

    [HttpGet]
    public IActionResult RegisterUser()
    {
        return View("Registration");
    }
}