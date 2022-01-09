using System.Net.Http;
using System.Threading.Tasks;
using LWS_GatewayIntegrationTest.Integration;
using Xunit;

namespace LWS_GatewayIntegrationTest.Controllers;

[Collection("DockerIntegration")]
public class MiscControllerTest
{
    private readonly HttpClient _httpClient;

    private readonly ServerFactory _serverFactory;

    public MiscControllerTest()
    {
        // Create Test Server
        _serverFactory = new ServerFactory();
        _httpClient = _serverFactory.CreateClient();
    }

    [Fact(DisplayName = "GET /api/misc/init should return 200 Ok")]
    public async Task Is_HealthCheck_Works_Well()
    {
        var response = await _httpClient.GetAsync("/api/misc/init");
        
        Assert.True(response.IsSuccessStatusCode);
    }
}