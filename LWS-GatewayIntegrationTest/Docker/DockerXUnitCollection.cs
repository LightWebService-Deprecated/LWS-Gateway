using Xunit;

namespace LWS_GatewayIntegrationTest.Docker;

[CollectionDefinition("DockerIntegration")]
public class DockerXUnitCollection: ICollectionFixture<DockerRunner>
{
}