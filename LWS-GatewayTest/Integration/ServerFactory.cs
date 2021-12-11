using System;
using System.Linq;
using LWS_Gateway;
using LWS_Gateway.Configuration;
using LWS_Gateway.Kube;
using LWS_Gateway.Repository;
using LWS_Gateway.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace LWS_GatewayTest.Integration;

public class ServerFactory: WebApplicationFactory<Startup>
{
    public readonly MongoContext IntegrationMongoContext;
    public readonly IKubernetesService KubernetesService;

    public ServerFactory()
    {
        IntegrationMongoContext = new MongoContext(new MongoConfiguration
        {
            MongoConnection = "mongodb://root:testPassword@localhost:27017",
            MongoDbName = Guid.NewGuid().ToString()
        });

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(a => a["KubePath"])
            .Returns("/tmp/kubeconfig.yaml");

        KubernetesService = new KubernetesService(mockConfiguration.Object, new ServiceDeploymentProvider());
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
            
            // Remove IKubernetes Service
            var kubeDescriptor = services.SingleOrDefault(a => a.ServiceType == typeof(IKubernetesService))
                                 ?? throw new NullReferenceException("Kubernetes object is not found on DI Container!");
            services.Remove(kubeDescriptor);

            services.AddScoped<IKubernetesService>(a => KubernetesService);
        });
    }
}