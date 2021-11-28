using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace LWS_GatewayTest.Docker.ContainerDefinition;

public class MongoDbContainer: DockerImageBase
{
    public MongoDbContainer(DockerClient dockerClient) : base(dockerClient)
    {
        ImageName = "mongo";
        ImageTag = "latest";
        ContainerParameters = new()
        {
            Image = FullImageName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["27017"] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = "27017"}}
                }
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                ["27017"] = new()
            },
            Env = new List<string>
            {
                "MONGO_INITDB_ROOT_USERNAME=root",
                "MONGO_INITDB_ROOT_PASSWORD=testPassword"
            }
        };
    }

    public override async Task RunContainerAsync()
    {
        await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
    }

    public override async Task CreateContainerAsync()
    {
        if (!await CheckImageExists())
        {
            // Pull from official Registry
            await DockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = ImageName,
                Tag = ImageTag
            }, new AuthConfig(), new Progress<JSONMessage>());
        }
        ContainerId = (await DockerClient.Containers.CreateContainerAsync(ContainerParameters))
            .ID;
    }

    public override async Task RemoveContainerAsync()
    {
        await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters());
        await DockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters());
    }
}