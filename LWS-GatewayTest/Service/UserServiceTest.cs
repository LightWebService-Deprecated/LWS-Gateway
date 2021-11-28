using System;
using System.Reflection;
using System.Runtime.Serialization;
using LWS_Gateway.CustomException;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Repository;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace LWS_GatewayTest.Service;

public class UserServiceTest
{
    private readonly UserService _userService;
    private readonly Mock<IAccountRepository> _mockAccountRepository;

    public UserServiceTest()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();

        _userService = new UserService(NullLogger<AuthenticationService>.Instance, _mockAccountRepository.Object);
    }
    
    private MongoWriteException CreateMongoException(ServerErrorCategory category)
    {
        var writeError = (WriteError) FormatterServices.GetUninitializedObject(typeof(WriteError));
        var writeErrorCategory =
            typeof(WriteError).GetField("_category", BindingFlags.NonPublic | BindingFlags.Instance);
        writeErrorCategory?.SetValue(writeError, category);
            
        var exceptionInfo = (MongoWriteException)FormatterServices.GetUninitializedObject(typeof(MongoWriteException));
        var toSet = typeof(MongoWriteException).GetField("_writeError", BindingFlags.NonPublic | BindingFlags.Instance);
        toSet.SetValue(exceptionInfo, writeError);

        return exceptionInfo;
    }

    [Fact(DisplayName = "RegisterRequest: RegisterRequest should handle conflict exception when occurred.")]
    public async void Is_RegisterRequest_Handles_Conflict_Exception()
    {
        // Let
        _mockAccountRepository.Setup(a => a.CreateAccountAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(CreateMongoException(ServerErrorCategory.DuplicateKey));
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        
        // Do
        await Assert.ThrowsAsync<ApiServerException>(() => _userService.RegisterRequest(registerRequest));
    }
    
    [Fact(DisplayName = "RegisterRequest: RegisterRequest should handle Other Mongo exception when occurred.")]
    public async void Is_RegisterRequest_Handles_Other_Mongo_Exception()
    {
        // Let
        _mockAccountRepository.Setup(a => a.CreateAccountAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(CreateMongoException(ServerErrorCategory.Uncategorized));
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        
        // Do
        await Assert.ThrowsAsync<ApiServerException>(() => _userService.RegisterRequest(registerRequest));
    }
    
    [Fact(DisplayName = "RegisterRequest: RegisterRequest should handle Other exception when occurred.")]
    public async void Is_RegisterRequest_Handles_Other_Exception()
    {
        // Let
        _mockAccountRepository.Setup(a => a.CreateAccountAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(new Exception());
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        
        // Do
        await Assert.ThrowsAsync<ApiServerException>(() => _userService.RegisterRequest(registerRequest));
    }

    [Fact(DisplayName = "RegisterRequest: RegisterRequest should work well.")]
    public async void Is_RegisterRequest_Works_Well()
    {
        // Let
        _mockAccountRepository.Setup(a => a.CreateAccountAsync(It.IsAny<RegisterRequest>()));
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        
        // Do
        await _userService.RegisterRequest(registerRequest);
        
        // Check
        _mockAccountRepository.Verify(a => a.CreateAccountAsync(It.IsAny<RegisterRequest>()));
    }

    [Fact(DisplayName = "LoginRequest should throw ApiServerException when login credential is incorrect.")]
    public async void Is_LoginRequest_Throws_ApiServerException_When_Login_Credential_Is_Incorrect()
    {
        // Let
        _mockAccountRepository.Setup(a => a.LoginAccountAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(value: null);
        var loginRequest = new LoginRequest
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        
        // Do
        await Assert.ThrowsAsync<ApiServerException>(() => _userService.LoginRequest(loginRequest));
    }

    [Fact(DisplayName = "LoginRequest: LoginRequest should return access token if login succeeds.")]
    public async void Is_LoginRequest_Works_Well()
    {
        // Let
        var account = new Account
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        _mockAccountRepository.Setup(a => a.LoginAccountAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(account);
        _mockAccountRepository.Setup(a => a.SaveAccessTokenAsync(account.UserEmail, It.IsAny<AccessToken>()));
        
        var loginRequest = new LoginRequest
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        
        // Do
        var accessToken = await _userService.LoginRequest(loginRequest);
        
        // Check
        Assert.NotNull(accessToken);
        _mockAccountRepository.Verify(a => a.SaveAccessTokenAsync(account.UserEmail, It.IsAny<AccessToken>()));
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
        var result = await _userService.AuthenticateUserRequest(authenticationRequest);
        
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
        var result = await _userService.AuthenticateUserRequest(authenticationRequest);
        
        // Check
        _mockAccountRepository.Verify(a => a.AuthenticateUserAsync(authenticationRequest.UserToken));
        Assert.Null(result);
    }

    [Fact(DisplayName = "DropoutUserRequest: DropoutUserRequest should remove user well.")]
    public async void Is_DropoutUserRequest_Works_Well()
    {
        // Let
        var userEmail = "test";
        _mockAccountRepository.Setup(a => a.DropoutUserAsync(userEmail));
        
        // Do
        await _userService.DropoutUserRequest(userEmail);
        
        // Check
        _mockAccountRepository.Verify((a => a.DropoutUserAsync(userEmail)));
    }
}