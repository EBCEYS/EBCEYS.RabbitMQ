using RabbitMQ.Client.Events;
using RabbitMQ.Server.MapedService.Attributes;
using RabbitMQ.Server.MapedService.Controllers;
using RabbitMQ.Server.MapedService.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Server.MapedService.RabbitMQControllerBaseTest
{
    [TestClass]
    public class RabbitMQControllerBaseTest
    {
        [TestMethod]
        public void CtorTest()
        {
            using TestRabbitMQController controller = new();
            Assert.IsNotNull(controller);
            Assert.IsTrue(controller.RabbitMQMethods!.Any());
        }
        [TestMethod]
        public void FindTestMethodTest_Exists()
        {
            using TestRabbitMQController controller = new();
            MethodInfo? method = controller.FindMethod("TestTaskMethod");
            Assert.IsNotNull(method);
        }
        [TestMethod]
        public void FindTestMethodTest_NotExists()
        {
            using TestRabbitMQController controller = new();
            MethodInfo? method = controller.FindMethod("TestMethod1");
            Assert.IsNull(method);
        }
        [TestMethod]
        public void GetMethodToExecuteTest_Exists() 
        { 
            using TestRabbitMQController controller = new();

            RabbitMQRequestData requestData = new()
            {
                Method = "TestTaskMethod"
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);
            Assert.IsNotNull(method);
        }
        [TestMethod]
        public void GetMethodToExecuteTest_NotExists()
        {
            using TestRabbitMQController controller = new();

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethod1"
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);
            Assert.IsNull(method);
        }
        [TestMethod]
        public async Task ProcessRequestAsyncTest()
        {
            using TestRabbitMQController controller = new();

            RabbitMQRequestData requestData = new()
            {
                Method = "TestTaskMethod"
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            await controller.ProcessRequestAsync(method!);

            Assert.IsNotNull(method);
        }
        [TestMethod]
        public async Task ProcessRequestWithResponseAsyncTest()
        {
            using TestRabbitMQController controller = new();

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithResponse"
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(int));

            Assert.IsNotNull(value);
            Assert.IsTrue((int)value == 0);
        }
        [TestMethod]
        public async Task ProcessRequestWithArgumentsAsyncTest()
        {
            using TestRabbitMQController controller = new();

            int[] arr = { 100, 200 };
            var arrStr = JsonSerializer.Serialize(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArguments",
                Params = JsonSerializer.Deserialize<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            await controller.ProcessRequestAsync(method!);
        }
        [TestMethod]
        public async Task ProcessRequestWithArgumentsAndResponseAsyncTest()
        {
            using TestRabbitMQController controller = new();

            int[] arr = { 50, 200 };
            var arrStr = JsonSerializer.Serialize(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArgumentsAndResponse",
                Params = JsonSerializer.Deserialize<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(int));

            Assert.IsNotNull(value);
            Assert.IsTrue((int)value == arr.Sum());
        }

        [TestMethod]
        public async Task ProcessRequestWithClassTest()
        {
            using TestRabbitMQController controller = new();

            TestAtr atr = new()
            {
                Val1 = 1,
                Val2 = 2
            };
            TestAtr[] arr = new[] { atr };
            string arrStr = JsonSerializer.Serialize(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithClassAttr",
                Params = JsonSerializer.Deserialize<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(int));

            Assert.IsNotNull(value);
            Assert.IsTrue(((int)value) == atr.Sum());
        }
    }

    internal class TestRabbitMQController : RabbitMQControllerBase
    {
        public TestRabbitMQController()
        {

        }
        [RabbitMQMethodName("TestTaskMethod")]
        public async Task TestTaskMethod()
        {
            await Task.Delay(500);
        }
        [RabbitMQMethodName("TestMethodWithResponse")]
        public async Task<int> TestTaskMethodWithResponse()
        {
            await Task.Delay(600);
            return 0;
        }
        [RabbitMQMethodName("TestMethodWithArguments")]
        public async Task TestTaskMethodWithArguments(int a, int b)
        {
            await Task.Delay(a);
            await Task.Delay(b);
        }
        [RabbitMQMethodName("TestMethodWithArgumentsAndResponse")]
        public async Task<int> TestTaskMethodWithArgumentsAndResponse(int a, int b)
        {
            return a + b;
        }
        [RabbitMQMethodName("TestMethodWithClassAttr")]
        public async Task<int> TestTaskMethodWithArgumentsAndResponse(TestAtr atr)
        {
            return atr.Sum();
        }
    }

    internal class TestAtr
    {
        public int Val1 { get; set; }
        public int Val2 { get; set; }

        public int Sum()
        {
            return Val1 + Val2;
        }
    }
}