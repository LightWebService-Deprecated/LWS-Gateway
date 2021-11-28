using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace LWS_Gateway.Extension
{
    [ExcludeFromCodeCoverage]
    public static class HttpClientExtension
    {
        public static void SetupNodeKey(this HttpClient httpClient, string nodeKey)
        {
            httpClient.DefaultRequestHeaders.Add("X-NODE-AUTH", new []{nodeKey});
        }
    }
}