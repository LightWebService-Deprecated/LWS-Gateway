using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Repository;
using LWS_Gateway.Service;
using LWS_GatewayIntegrationTest.Integration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace LWS_GatewayIntegrationTest.Controllers;

[Collection("DockerIntegration")]
public class UserControllerTest: IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IMongoCollection<Account> _mongoCollection;
    private readonly IKubernetesService _kubernetesService;

    private readonly ServerFactory _serverFactory;

    public UserControllerTest()
    {
        // Create Test Server
        _serverFactory = new ServerFactory();
        _httpClient = _serverFactory.CreateClient();
        
        // Get DI Services
        var serviceProvider = _serverFactory.Services;
        
        // Get Database Information
        var mongoContext = serviceProvider.GetService<MongoContext>()
                           ?? throw new NullReferenceException("MongoContext is not registered in DI Container!");
        _mongoCollection = mongoContext.MongoDatabase.GetCollection<Account>(nameof(Account));
        _kubernetesService = _serverFactory.KubernetesService;
    }

    public void Dispose()
    {
        _serverFactory.Dispose();
    }

    [Fact(DisplayName = "POST /api/user (register) should return 200 OK.")]
    public async void Is_CreateUser_Returns_OK()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        
        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user", registerRequest);
        
        // Check
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/user (register) should return 409 CONFLICT.")]
    public async void Is_CreateUser_Returns_Conflict()
    {
        // Let
        var account = new Account
        {
            UserEmail = "test",
            UserPassword = "testPassword"
        };
        await _mongoCollection.InsertOneAsync(account);
        var registerRequest = new RegisterRequest
        {
            UserEmail = account.UserEmail,
            UserPassword = account.UserPassword
        };
        
        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user", registerRequest);
        
        // Check
        Assert.NotNull(response);
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/user/login (login) should return 200 OK with access token.")]
    public async void Is_LoginUser_Returns_Ok()
    {
        // Let
        var account = new Account
        {
            UserEmail = "test",
            UserPassword = "testPassword",
            UserAccessTokens = new List<AccessToken>()
        };
        await _mongoCollection.InsertOneAsync(account);
        var loginRequest = new LoginRequest()
        {
            UserEmail = account.UserEmail,
            UserPassword = account.UserPassword
        };
        
        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/login", loginRequest);
        
        // Check
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/user/login (login) should return 401 UNAUTHORIZED")]
    public async void Is_LoginUser_Returns_Unauthorized()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "testUser",
            UserPassword = "testPassword"
        };
        
        // Do
        var response = await _httpClient.PostAsJsonAsync("/api/user/login", loginRequest);
        
        // Check
        Assert.NotNull(response);
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /api/user (Dropout) should return 401 UNAUTHORIZED when access token is not provided.")]
    public async void Is_DropoutUser_Returns_Unauthorized_When_AccessToken_Not_Provided()
    {
        // Do
        var response = await _httpClient.DeleteAsync("/api/user");
        
        // Check
        Assert.NotNull(response);
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }
    
    [Fact(DisplayName = "DELETE /api/user (Dropout) should return 200 OK.")]
    public async void Is_DropoutUser_Works_Well()
    {
        // Let
        var account = new Account
        {
            UserEmail = "test",
            UserPassword = "testPassword",
            UserAccessTokens = new List<AccessToken>
            {
                new()
                {
                    CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    ExpiresAt = DateTimeOffset.Now.AddDays(10).ToUnixTimeSeconds(),
                    Token = "test"
                }
            }
        };
        await _mongoCollection.InsertOneAsync(account);
        await _kubernetesService.CreateNameSpace(account.Id);
        _httpClient.DefaultRequestHeaders.Add("X-API-AUTH", new []{account.UserAccessTokens[0].Token});
        
        // Do
        var response = await _httpClient.DeleteAsync("/api/user");
        
        // Check
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact(DisplayName = "GET /api/user (Get User Projection) should return 200 OK.")]
    public async void Is_GetUserProjection_Returns_200()
    {
        // let
        var account = new Account
        {
            UserEmail = "test",
            UserPassword = "testPassword",
            UserAccessTokens = new List<AccessToken>
            {
                new()
                {
                    CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    ExpiresAt = DateTimeOffset.Now.AddDays(10).ToUnixTimeSeconds(),
                    Token = "test"
                }
            }
        };
        await _mongoCollection.InsertOneAsync(account);
        await _kubernetesService.CreateNameSpace(account.Id);
        _httpClient.DefaultRequestHeaders.Add("X-API-AUTH", new []{account.UserAccessTokens[0].Token});
        
        // Do
        var response = await _httpClient.GetAsync("/api/user");
        
        // Check
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
    }
}