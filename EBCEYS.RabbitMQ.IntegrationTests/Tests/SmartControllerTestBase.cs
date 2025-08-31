using EBCEYS.RabbitMQ.Client;
using NSubstitute;

namespace EBCEYS.RabbitMQ.IntegrationTests.Tests;

public abstract class SmartControllerTestBase
{
    protected IRabbitMQClient Client = new RabbitMQClient(Substitute.For<ILogger<RabbitMQClient>>(), TestsContext.GetConfiguration());
    protected IRabbitMQClient GZipedClient = new RabbitMQClient(Substitute.For<ILogger<RabbitMQClient>>(), TestsContext.GetConfigurationGZiped());

    protected async Task ResetAsync()
    {
        Client = new RabbitMQClient(Substitute.For<ILogger<RabbitMQClient>>(), TestsContext.GetConfiguration());
        GZipedClient = new RabbitMQClient(Substitute.For<ILogger<RabbitMQClient>>(), TestsContext.GetConfigurationGZiped());
        await Client.StartAsync(CancellationToken.None);
        await GZipedClient.StartAsync(CancellationToken.None);
    }
}