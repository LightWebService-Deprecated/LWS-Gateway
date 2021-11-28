using Xunit;

namespace LWS_GatewayTest.Docker;

[CollectionDefinition("DockerIntegration")]
public class DockerXUnitCollection: ICollectionFixture<DockerRunner>
{
}