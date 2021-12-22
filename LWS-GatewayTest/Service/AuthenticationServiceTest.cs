using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Repository;
using LWS_Gateway.Service;
using Moq;
using Xunit;

namespace LWS_GatewayTest.Service;

public class AuthenticationServiceTest
{
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTest()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _authenticationService = new AuthenticationService(_mockAccountRepository.Object);
    }
    
    [Fact(DisplayName =
        "AuthenticateUserRequest: AuthenticateUserRequest should find appropriate account information.")]
    public async void Is_AuthenticateUserRequest_Returns_Correct_Account()
    {
        // Let
        var authenticationRequest = new AuthenticationRequest
        {
            UserToken = "test"
        };
        _mockAccountRepository.Setup(a => a.AuthenticateUserAsync(authenticationRequest.UserToken))
            .ReturnsAsync(new Account());
        
        // Do
        var result = await _authenticationService.AuthenticateUserRequest(authenticationRequest);
        
        // Check
        _mockAccountRepository.Verify(a => a.AuthenticateUserAsync(authenticationRequest.UserToken));
        Assert.NotNull(result);
    }

    [Fact(DisplayName = "AuthenticateUserRequest: AuthenticateUserRequest should return null when verification fails.")]
    public async void Is_AuthenticateUserRequest_Returns_Null_When_Verification_Fails()
    {
        // Let
        var authenticationRequest = new AuthenticationRequest
        {
            UserToken = "test"
        };
        _mockAccountRepository.Setup(a => a.AuthenticateUserAsync(authenticationRequest.UserToken))
            .ReturnsAsync(value: null);
        
        // Do
        var result = await _authenticationService.AuthenticateUserRequest(authenticationRequest);
        
        // Check
        _mockAccountRepository.Verify(a => a.AuthenticateUserAsync(authenticationRequest.UserToken));
        Assert.Null(result);
    }
}