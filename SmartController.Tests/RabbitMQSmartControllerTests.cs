using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace SmartController.Tests
{
    [TestClass]
    public class RabbitMQSmartControllerTests
    {
        private async Task<(RabbitMQClient, TestSmartController)> CreateTestingObjects()
        {
            RabbitMQConfigurationBuilder configBuilder = new();
            configBuilder.AddConnectionFactory(new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5673
            });
            configBuilder.AddQueueConfiguration(new("TestQueue", autoDelete: true));
            configBuilder.AddCallbackConfiguration(new(new("testqueue_callback", autoDelete: true)));
            RabbitMQConfiguration config = configBuilder.Build();
#pragma warning disable CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
            TestSmartController testController = RabbitMQSmartControllerBase.InitializeNewController<TestSmartController>(config, null);
#pragma warning restore CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
            await testController.StartAsync(CancellationToken.None);
            RabbitMQClient testClient = new(new NullLoggerFactory().CreateLogger<RabbitMQClient>(), config, TimeSpan.FromSeconds(10.0));
            await testClient.StartAsync(CancellationToken.None);
            return (testClient, testController);
        }
        [TestMethod]
        public async Task TestSendMessage()
        {
            (RabbitMQClient? testClient, TestSmartController? testController) = await CreateTestingObjects();
            await testClient.SendMessageAsync(new()
            {
                Method = "TestMethod1",
                Params = ["a", "b"]
            });
        }
        [TestMethod]
        public async Task SendRequestSum()
        {
            (RabbitMQClient? testClient, TestSmartController? testController) = await CreateTestingObjects();
            object[] @params = [1, 2];
            long? result = await testClient!.SendRequestAsync<long>(new()
            {
                Method = "TestMethodSumRequest",
                Params = @params
            });
            Assert.IsNotNull(result);
            Assert.AreEqual(@params.Sum(Convert.ToInt64), result);
        }
        [TestMethod]
        public async Task SendRequestWithDto()
        {
            (RabbitMQClient? testClient, TestSmartController? testController) = await CreateTestingObjects();
            TestDto dto = new()
            {
                Id = 1,
                Name = "1",
                Data = [1, 2]
            };
            TestDto? result = await testClient!.SendRequestAsync<TestDto>(new()
            {
                Method = "TestMethodDtoRequest",
                Params = [dto]
            });
            Assert.IsNotNull(result);
            Assert.AreEqual(dto.CreateHash(), result.CreateHash(), true);
        }
        [TestMethod]
        public async Task SendRequestSumWithOneMoreParam()
        {
            (RabbitMQClient? testClient, TestSmartController? testController) = await CreateTestingObjects();
            object[] @params = [1, 2, 3];
            Assert.ThrowsException<AggregateException>(() => testClient!.SendRequestAsync<long>(new()
            {
                Method = "TestMethodSumRequest",
                Params = @params
            }).Wait());
        }
        [TestMethod]
        public async Task SendRequestWithException()
        {
            (RabbitMQClient? testClient, TestSmartController? testController) = await CreateTestingObjects();
            object[] @params = ["test"];
            Assert.ThrowsException<AggregateException>(() => testClient!.SendRequestAsync<long>(new()
            {
                Method = "TestMethodWithException",
                Params = @params
            }).Wait());
        }
        [TestMethod]
        public async Task SendRequestWithInnerException()
        {
            (RabbitMQClient? testClient, TestSmartController? testController) = await CreateTestingObjects();
            object[] @params = ["testInner", "test"];
            Assert.ThrowsException<AggregateException>(() => testClient!.SendRequestAsync<long>(new()
            {
                Method = "TestMethodWithInnerException",
                Params = @params
            }).Wait());
        }
    }
    
    public class TestDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public byte[]? Data { get; set; }
        public string CreateHash()
        {
            StringBuilder sb = new();
            sb.Append(string.Empty);
            foreach (byte b in Data ?? [])
            {
                sb.Append(b);
            }
            return Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes($"{Id}{Name}{sb}")));
        }
    }

    internal class TestSmartController : RabbitMQSmartControllerBase
    {
        [RabbitMQMethod("TestMethod1")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<string> TestMethod1(string a, string b)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(a + b);
        }

        [RabbitMQMethod("TestMethod2")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task TestMethod2(string a, string b)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            _ = a + b;
            return Task.CompletedTask;
        }

        [RabbitMQMethod("TestMethodSumRequest")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<long> TestRequest1(long a, long b)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(a + b);
        }

        [RabbitMQMethod("TestMethodDtoRequest")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<TestDto> TestRequest2(TestDto dto)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(dto);
        }
        [RabbitMQMethod("TestMethodWithException")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<object> TestRequestException1(string name)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            throw new RabbitMQRequestProcessingException(name, new Exception());
        }
        [RabbitMQMethod("TestMethodWithInnerException")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<object> TestRequestException2(string name1, string name2)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            try
            {
                throw new InvalidOperationException(name1);
            }
            catch (Exception ex)
            {
                throw new RabbitMQRequestProcessingException(name2, ex);
            }
        }
    }
}