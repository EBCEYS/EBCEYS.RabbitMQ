using System.Security.Cryptography;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using EBCEYS.RabbitMQ.TestServer;
using FluentAssertions;

namespace EBCEYS.RabbitMQ.IntegrationTests.Tests;

public class ControllerTests : SmartControllerTestBase
{
    [OneTimeSetUp]
    public async Task Setup()
    {
        await ResetAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Client.StopAsync(CancellationToken.None);
        await GZipedClient.StopAsync(CancellationToken.None);
    }

    [TestCase(5, 5)]
    [TestCase(10, 10)]
    [TestCase(16, 16)]
    [Repeat(100)]
    [MaxTime(3000)]
    public async Task When_SendMessage_Result_Ok(int l1, int l2)
    {
        var str1 = GetRandomString(l1);
        var str2 = GetRandomString(l2);
        var res = str1 + str2;
        Console.WriteLine($"Expected res is: {res}");
        var tcs = new TaskCompletionSource<string>();

        TestsContext.Tasks.TryAdd(res, tcs);
        await Client.SendMessageAsync(new RabbitMQRequestData
        {
            Method = "TestMethod1",
            Params = [str1, str2]
        });

        var result = await tcs.Task;
        result.Should().Be(res);
        WasAnyMessageReceivedMiddleware.WasAnyMessageReceived.Should().BeTrue();
    }

    [TestCase(5, 5)]
    [TestCase(10, 10)]
    [TestCase(16, 16)]
    [Repeat(100)]
    public async Task When_SendRequest_Result_Ok(int l1, int l2)
    {
        var str1 = GetRandomString(l1);
        var str2 = GetRandomString(l2);

        var response = await Client.SendRequestAsync<string>(new RabbitMQRequestData
        {
            Method = "ExampleMethod",
            Params = [str1, str2]
        });

        response.Should().Be(str1 + str2);
    }

    [TestCase(5, 5)]
    [TestCase(10, 10)]
    [TestCase(16, 16)]
    [Repeat(100)]
    public async Task When_GZipedSendRequest_Result_Ok(int l1, int l2)
    {
        var str1 = GetRandomString(l1);
        var str2 = GetRandomString(l2);

        var response = await GZipedClient.SendRequestAsync<string>(new RabbitMQRequestData
        {
            Method = "ExampleMethod",
            Params = [str1, str2],
            GZip = new GZipSettings(true)
        });

        response.Should().Be(str1 + str2);
        WasAnyMessageReceivedMiddleware.WasAnyMessageReceived.Should().BeTrue();
    }

    [Test]
    [Repeat(100)]
    public async Task When_ServicedRequest_Result_Ok()
    {
        var str = GetRandomString(5);
        var response = await Client.SendRequestAsync<string>(new RabbitMQRequestData
        {
            Method = "ServiceMethod",
            Params = [str]
        });

        response.Should().StartWith(MyCustomService.Prefix).And.EndWith(str);
        WasAnyMessageReceivedMiddleware.WasAnyMessageReceived.Should().BeTrue();
    }

    [Test]
    [Repeat(100)]
    public async Task When_KeyedServicedRequest_Result_Ok()
    {
        var str = GetRandomString(5);
        var response = await Client.SendRequestAsync<string>(new RabbitMQRequestData
        {
            Method = "KeyedServiceMethod",
            Params = [str]
        });

        response.Should().StartWith(MyCustomKeyedService.Prefix).And.EndWith(str);
        WasAnyMessageReceivedMiddleware.WasAnyMessageReceived.Should().BeTrue();
    }

    private static string GetRandomString(int length)
    {
        return RandomNumberGenerator.GetHexString(length);
    }
}