using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Deployment;
using LWS_Gateway.Model.Request;
using LWS_GatewayIntegrationTest.Extension;
using LWS_GatewayIntegrationTest.Integration;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
using Xunit;

namespace LWS_GatewayIntegrationTest.Controllers;

[Collection("DockerIntegration")]
public class DeploymentControllerTest : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ServerFactory _testServer;
    
    public DeploymentControllerTest()
    {
        _testServer = new ServerFactory();
        _httpClient = _testServer.CreateClient();
    }

    public void Dispose()
    {
        _testServer.Dispose();
    }

    private (RegisterRequest, LoginRequest) CreateRandomCredential()
    {
        var registerRequest = new RegisterRequest
        {
            UserEmail = Guid.NewGuid().ToString(),
            UserPassword = Guid.NewGuid().ToString()
        };
        var loginRequest = new LoginRequest
        {
            UserEmail = registerRequest.UserEmail,
            UserPassword = registerRequest.UserPassword
        };

        return (registerRequest, loginRequest);
    }

    private async Task<string> CreateRandomUserAsync()
    {
        // Create User Credential
        var (registerRequest, loginRequest) = CreateRandomCredential();
        
        // Register
        var registerResponse = await _httpClient.PostAsJsonAsync("/api/user", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        
        // Login
        var loginResponse = await _httpClient.PostAsJsonAsync("/api/user/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        
        // Get Access Token
        var accessToken = (await loginResponse.Content.ReadFromJsonAsync<AccessToken>())
                          ?? throw new NullReferenceException(
                              "Cannot deserialize login response to access token object!");
        
        return accessToken.Token!;
    }

    [Theory(DisplayName = "POST /api/v1/deployment should create desired deployment well.")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public async void Is_CreateDeployment_Returns_Definition_Well(int deploymentCount)
    {
        // Let
        var accessToken = await CreateRandomUserAsync();

        for (var i = 0; i < deploymentCount; i++)
        {
            var deploymentRequest = new DeploymentCreationRequest
            {
                DeploymentType = DeploymentType.Ubuntu
            };
        
            // Do
            _httpClient.AddAuthToken(accessToken);
            var responseMessage = await _httpClient.PostAsJsonAsync("/api/v1/deployment", deploymentRequest);
        
            // Check
            responseMessage.EnsureSuccessStatusCode();
            var responseBody = await responseMessage.Content.ReadFromJsonAsync<DeploymentDefinition>();
            Assert.NotNull(responseBody);
            Assert.Contains("-ubuntu-", responseBody.DeploymentName);
            Assert.Equal(DeploymentType.Ubuntu, responseBody.DeploymentType);
            Assert.Equal(2, responseBody.DeploymentOpenedPorts.Count);
            Assert.Contains(22, responseBody.DeploymentOpenedPorts);
        }
    }

    [Fact(DisplayName = "POST /api/v1/deployment should return 401 unauthorized when no token provided.")]
    public async void Is_CreateDeployment_Returns_Unauthorized_When_No_Token()
    {
        // Let
        var deploymentRequest = new DeploymentCreationRequest
        {
            DeploymentType = DeploymentType.Ubuntu
        };
        
        // Do
        var responseMessage = await _httpClient.PostAsJsonAsync("/api/v1/deployment", deploymentRequest);
        Assert.False(responseMessage.IsSuccessStatusCode);
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)responseMessage.StatusCode);
    }

    [Fact(DisplayName = "DELETE /api/v1/deployment should remove deployment well.")]
    public async void Is_DeleteDeployment_Works_Well()
    {
        // Let
        var accessToken = await CreateRandomUserAsync();
        var deploymentRequest = new DeploymentCreationRequest
        {
            DeploymentType = DeploymentType.Ubuntu
        };
        _httpClient.AddAuthToken(accessToken);
        var responseSecond = await _httpClient.PostAsJsonAsync("/api/v1/deployment", deploymentRequest);
        var response = await responseSecond.Content.ReadFromJsonAsync<DeploymentDefinition>();
        var deleteRequest = new DeploymentDeleteRequest
        {
            DeploymentName = response.DeploymentName
        };
        
        // Do
        var requestMessage = new HttpRequestMessage
        {
            RequestUri = new Uri("/api/v1/deployment"),
            Method = HttpMethod.Delete,
            Content = JsonContent.Create(deleteRequest, mediaType:null, null)
        };
        requestMessage.Headers.Add("X-API-AUTH", accessToken);
        
        _httpClient.AddAuthToken(accessToken);
        var deleteResponse = await _httpClient.SendAsync(requestMessage);
        
        // Check
        deleteResponse.EnsureSuccessStatusCode();
        Assert.True(deleteResponse.IsSuccessStatusCode);
        Assert.Equal(StatusCodes.Status200OK, (int)deleteResponse.StatusCode);
    }

    [Fact(DisplayName =
        "DELETE /api/v1/deployment should return 404 not found when user tried to remove non-exist deployment.")]
    public async void Is_DeleteDeployment_Returns_404_When_Removing_Non_Exists_Deployment()
    {
        // Let
        var accessToken = await CreateRandomUserAsync();
        _httpClient.AddAuthToken(accessToken);
        var deleteRequest = new DeploymentDeleteRequest
        {
            DeploymentName = "response.DeploymentName"
        };
        
        // Do
        var deleteResponse = await _httpClient.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            Content = new StringContent(content: JsonConvert.SerializeObject(deleteRequest), Encoding.UTF8,
                "application/json")
        });
        
        // Check
        Assert.Equal(StatusCodes.Status404NotFound, (int)deleteResponse.StatusCode);
    }
    
    [Fact(DisplayName =
        "DELETE /api/v1/deployment should return 401 unauthorized when no token supplied.")]
    public async void Is_DeleteDeployment_Returns_401_When_No_Token()
    {
        // Let
        var deleteRequest = new DeploymentDeleteRequest
        {
            DeploymentName = "response.DeploymentName"
        };
        
        // Do
        var deleteResponse = await _httpClient.SendAsync(new HttpRequestMessage
        {
            RequestUri = new Uri("/api/v1/deployment"),
            Method = HttpMethod.Delete,
            Content = new StringContent(content: JsonConvert.SerializeObject(deleteRequest), Encoding.UTF8,
                "application/json")
        });
        
        // Check
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)deleteResponse.StatusCode);
    }

    [Fact(DisplayName = "GET /api/v1/deployment should return list of deployments with 200 OK.")]
    public async void Is_GetDeployment_Returns_200_With_Lists()
    {
        // Let
        var accessToken = await CreateRandomUserAsync();
        _httpClient.AddAuthToken(accessToken);
        
        // Do
        var result = await _httpClient.GetAsync("/api/v1/deployment");
        
        // Check
        Assert.Equal(StatusCodes.Status200OK, (int)result.StatusCode);
    }
}