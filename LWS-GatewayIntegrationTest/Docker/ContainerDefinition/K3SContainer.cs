using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace LWS_GatewayTest.Docker.ContainerDefinition;

public class K3SContainer: DockerImageBase
{
    private const string _containerName = "INTGR-K3S";
    public K3SContainer(DockerClient dockerClient) : base(dockerClient)
    {
        ImageName = "rancher/k3s";
        ImageTag = "latest";

        ContainerParameters = new CreateContainerParameters
        {
            Image = FullImageName,
            Name = _containerName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["6443"] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = "6443"}},
                    ["80"] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = "80"}},
                    ["443"] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = "443"}},
                },
                Binds = new List<string>
                {
                    "/tmp:/output"
                },
                Privileged = true
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                ["6443"] = new(),
                ["80"] = new(),
                ["443"] = new()
            },
            Env = new List<string>
            {
                "K3S_KUBECONFIG_OUTPUT=/output/kubeconfig.yaml",
                "K3S_KUBECONFIG_MODE=666"
            },
            Cmd = new List<string>
            {
                "server"
            }
        };
    }

    public override async Task CreateContainerAsync()
    {
        var list = await DockerClient.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = true,
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                ["name"] = new Dictionary<string, bool>
                {
                    [_containerName] = true
                }
            }
        });

        if (list.Count > 0)
        {
            foreach (var eachContainer in list)
            {
                await DockerClient.Containers.StopContainerAsync(eachContainer.ID, new ContainerStopParameters());
                await DockerClient.Containers.RemoveContainerAsync(eachContainer.ID, new ContainerRemoveParameters());
            }
        }
        
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

    public override async Task RunContainerAsync()
    {
        await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
        Thread.Sleep(20 * 1000);
    }

    public override async Task RemoveContainerAsync()
    {
        await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters());
        await DockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters());
    }
}