using System.Security.Cryptography;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
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

    [TestCase(5,5)]
    [TestCase(10,10)]
    [TestCase(16,16)]
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
    }

    [TestCase(5,5)]
    [TestCase(10,10)]
    [TestCase(16,16)]
    public async Task When_SendRequest_Result_Ok(int l1, int l2)
    {
        var str1 = GetRandomString(l1);
        var str2 = GetRandomString(l2);

        var response = await Client.SendRequestAsync<string>(new RabbitMQRequestData
        {
            Method = "ExampleMethod",
            Params = [str1, str2],
        });

        response.Should().Be(str1 + str2);
    }
    
    [TestCase(5,5)]
    [TestCase(10,10)]
    [TestCase(16,16)]
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
    }

    private static string GetRandomString(int length)
    {
        return RandomNumberGenerator.GetHexString(length);
    }
}