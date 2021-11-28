using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LWS_Gateway.CustomException;
using LWS_Gateway.Management.Model;
using LWS_Gateway.Management.Model.Request;
using LWS_Gateway.Management.Repository;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LWS_Gateway.Management.Service
{
    [ExcludeFromCodeCoverage]
    public class NodeManageService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly HttpClient _httpClient;

        public NodeManageService(INodeRepository nodeRepository, IHttpClientFactory factory)
        {
            _nodeRepository = nodeRepository;
            _httpClient = factory.CreateClient();
        }

        private async Task RegisterHandler(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiServerException(StatusCodes.Status401Unauthorized,
                    "Cannot authorize node with defined key!");
            }

            var responseMessage = await response.Content.ReadAsStringAsync();
            throw new ApiServerException(StatusCodes.Status500InternalServerError, $"Unknown Error Occurred: {responseMessage}");
        }
        
        public async Task RegisterNewNodeInternal(NewNodeRequest nodeRequest)
        {
            // Setup Key
            _httpClient.DefaultRequestHeaders.Add("X-NODE-AUTH", new []{nodeRequest.SecretKey});
            
            // Get Response
            var response = await _httpClient.GetAsync($"{nodeRequest.NodeServerUrl}/api/v1/node/management");
            await RegisterHandler(response);
            
            // Get Node Metadata
            var nodeMetadata =
                JsonConvert.DeserializeObject<NodeConfiguration>(await response.Content.ReadAsStringAsync())
                ?? throw new NullReferenceException($"Tried to deserialize object, but it returned null!");

            // Add to database
            await _nodeRepository.AddNodeInfoAsync(new NodeInformation
            {
                NodeMaximumCpu = nodeMetadata.NodeMaximumCpu,
                NodeUrl = nodeRequest.NodeServerUrl,
                NodeMaximumRam = nodeMetadata.NodeMaximumRam,
                NodeNickName = nodeMetadata.NodeNickName,
                NodeAllocatedCpu = 0,
                NodeAllocatedRam = 0,
                NodeCpuUsage = 0.0,
                NodeRamUsage = 0.0,
                NodeKey = nodeMetadata.NodeKey
            });
        }
    }
}