using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using LWS_GatewayTest.Docker.ContainerDefinition;

namespace LWS_GatewayIntegrationTest.Docker;

public class DockerRunner: IDisposable
{
    private readonly DockerClient _dockerClient;
    private readonly List<DockerImageBase> _dockerImageList;

    public DockerRunner()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        _dockerImageList = new List<DockerImageBase>
        {
            new MongoDbContainer(_dockerClient),
            new K3SContainer(_dockerClient)
        };

        CreateAllContainer().Wait();
    }

    private async Task CreateAllContainer()
    {
        foreach (var eachContainer in _dockerImageList)
        {
            await eachContainer.CreateContainerAsync();
            await eachContainer.RunContainerAsync();
        }
    }

    private async Task RemoveAllContainer()
    {
        foreach (var eachContainer in _dockerImageList)
        {
            await eachContainer.RemoveContainerAsync();
        }
    }

    public void Dispose()
    {
        RemoveAllContainer().Wait();
        _dockerClient.Dispose();
    }
}