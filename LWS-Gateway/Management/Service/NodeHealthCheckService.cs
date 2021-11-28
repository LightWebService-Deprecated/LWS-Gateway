using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LWS_Gateway.CustomException;
using LWS_Gateway.Extension;
using LWS_Gateway.Management.Model;
using LWS_Gateway.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace LWS_Gateway.Management.Service
{
    [ExcludeFromCodeCoverage]
    public class NodeHealthCheckService: IHostedService
    {
        private readonly IMongoCollection<NodeInformation> _collection;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        private Timer _timer;
        
        public NodeHealthCheckService(MongoContext mongoContext, ILogger<NodeHealthCheckService> logger, IHttpClientFactory factory)
        {
            _collection = mongoContext.MongoDatabase.GetCollection<NodeInformation>(nameof(NodeInformation));
            _logger = logger;
            _httpClient = factory.CreateClient();
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async Task HandleHeartbeat(HttpResponseMessage response, NodeInformation eachNode)
        {
            if (response.IsSuccessStatusCode) return;
            
            var responseMessage = await response.Content.ReadAsStringAsync();
            _logger.LogCritical($"{eachNode.NodeUrl} is not responding! removing from node list.");
            _logger.LogCritical($"Response: {responseMessage}");

            var filter = Builders<NodeInformation>.Filter.Eq(a => a.Id, eachNode.Id);
            await _collection.DeleteOneAsync(filter);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiServerException(StatusCodes.Status401Unauthorized,
                    "Cannot authorize node with defined key!");
            }
            
            throw new ApiServerException(StatusCodes.Status500InternalServerError, $"Unknown Error Occurred: {responseMessage}");
        }

        private async void Callback(object? state)
        {
            // Get Node
            var list = await _collection.AsQueryable().ToListAsync();
            _logger.LogInformation("Heartbeat Started");
            
            // Check Each node's health
            foreach (var eachNode in list)
            {
                _logger.LogInformation($"Heartbeat node {eachNode.NodeNickName} / {eachNode.NodeUrl}");
                
                // Setup Header
                _httpClient.SetupNodeKey(eachNode.NodeKey);
                
                // Request
                var response = await _httpClient.GetAsync($"{eachNode.NodeUrl}/api/v1/node/management/alive");
                
                // Handle it
                await HandleHeartbeat(response, eachNode);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}