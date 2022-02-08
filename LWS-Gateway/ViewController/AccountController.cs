using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LWS_Gateway.CustomException;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions;

namespace LWS_Gateway.ViewController;

[Route("/account")]
public class AccountController: Controller
{
    private readonly UserService _userService;
    private readonly AuthenticationService _authenticationService;
    
    public AccountController(UserService userService, AuthenticationService authenticationService)
    {
        _userService = userService;
        _authenticationService = authenticationService;
    }

    [HttpGet("login")]
    public IActionResult LoginUser(string returnUrl)
    {
        return View();
    }

    [HttpGet]
    public IActionResult RegisterUser()
    {
        return View("Registration");
    }

    [Authorize]
    [HttpGet("info")]
    public async Task<IActionResult> AccountSettingsPage()
    {
        var token = HttpContext.User.Claims.First(a => a.Type == "token")
            .Value;
        var account = await _authenticationService.AuthenticateUserRequest(new AuthenticationRequest {UserToken = token});
        
        return View("AccountInfoEdit", account.ToViewHeaderResponse());
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginFromViewAsync(LoginRequest loginRequest, [FromQuery] string returnUrl)
    {
        // Login
        var token = await GetTokenFromLogin(loginRequest);

        if (token == null)
        {
            ViewBag.Error = true;
            return View("LoginUser");
        }
        
        // Get Account
        var account =
            await _authenticationService.AuthenticateUserRequest(new AuthenticationRequest {UserToken = token.Token});
        
        // Build Claim
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("userId", account.Id),
                new Claim("token", token.Token),
                new Claim(ClaimTypes.Email, account.UserEmail),
                new Claim(ClaimTypes.Expiration, $"{DateTimeOffset.UtcNow.AddMinutes(9).ToUnixTimeMilliseconds()}")
            }
        , CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction("Index", "Index");
        }
        
        return Redirect(returnUrl);
    }

    private async Task<AccessToken> GetTokenFromLogin(LoginRequest loginRequest)
    {
        AccessToken token = default(AccessToken);
        try
        {
            token = await _userService.LoginRequest(loginRequest);
        }
        catch (ApiServerException exception)
        {
            // Failed
        }

        return token;
    }
}