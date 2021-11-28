using System;
using System.Linq;
using LWS_Gateway;
using LWS_Gateway.Configuration;
using LWS_Gateway.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LWS_GatewayTest.Integration;

public class ServerFactory: WebApplicationFactory<Startup>
{
    public readonly MongoContext IntegrationMongoContext;

    public ServerFactory()
    {
        IntegrationMongoContext = new MongoContext(new MongoConfiguration
        {
            MongoConnection = "mongodb://root:testPassword@localhost:27017",
            MongoDbName = Guid.NewGuid().ToString()
        });
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove MongoContext
            var descriptor = services.SingleOrDefault(a => a.ServiceType == typeof(MongoContext))
                             ?? throw new NullReferenceException("MongoContext is not found on DI Container!");
            services.Remove(descriptor);
            
            // Register new MongoContext
            services.AddSingleton(IntegrationMongoContext);
        });
    }
}