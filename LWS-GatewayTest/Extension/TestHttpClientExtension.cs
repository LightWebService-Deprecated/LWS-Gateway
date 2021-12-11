using System.Collections.Generic;
using System.Net.Http;

namespace LWS_GatewayTest.Extension;

public static class TestHttpClientExtension
{
    public static void AddAuthToken(this HttpClient httpClient, string token)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("X-API-AUTH", new List<string> {token});
    }
}