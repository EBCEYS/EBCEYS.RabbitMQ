using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text;

namespace EBCEYS.RabbitMQ.Server.MappedService.RabbitMQControllerBaseTest
{
    [TestClass]
    public class RabbitMQControllerBaseTest
    {
        [TestMethod]
        public void CtorTest()
        {
            using TestRabbitMQController controller = new();
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
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

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
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

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
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

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
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.AreEqual(0, (long)value);
        }
        [TestMethod]
        public async Task ProcessRequestWithArgumentsAsyncTest()
        {
            using TestRabbitMQController controller = new();

            long[] arr = [100, 200];
            var arrStr = JsonConvert.SerializeObject(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArguments",
                Params = JsonConvert.DeserializeObject<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            await controller.ProcessRequestAsync(method!);
        }
        [TestMethod]
        public async Task ProcessRequestWithArgumentsAndResponseAsyncTest()
        {
            using TestRabbitMQController controller = new();

            long[] arr = [50, 200];
            var arrStr = JsonConvert.SerializeObject(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArgumentsAndResponse",
                Params = JsonConvert.DeserializeObject<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.AreEqual(arr.Sum(), (long)value);
        }

        [TestMethod]
        public async Task ProcessRequestWithArgumentsAndResponseAsync_String_Test()
        {
            using TestRabbitMQController controller = new();

            string[] arr = ["50", "200"];
            var arrStr = JsonConvert.SerializeObject(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArgumentsAndResponseString",
                Params = JsonConvert.DeserializeObject<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(string));

            Assert.IsNotNull(value);
            Assert.AreEqual(arr[0] + arr[1], (string)value);
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

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithClassAttr",
                Params = [atr]
            };
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.AreEqual(atr.Sum(), (long)value);
        }

        [TestMethod]
        public async Task ProcessRequestWithClassArrayTest()
        {
            using TestRabbitMQController controller = new();

            TestAtr atr = new()
            {
                Val1 = 1,
                Val2 = 2
            };
            TestAtr atr2 = new()
            {
                Val1 = 1,
                Val2 = 2
            };

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithClassAttrArray",
                Params = [new TestAtr[] { atr, atr2 }]
            };
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.AreEqual(atr.Sum() + atr2.Sum(), (long)value);
        }
        [TestMethod]
        public async Task ProcessRequestWithClassTwoArraysTest()
        {
            using TestRabbitMQController controller = new();

            TestAtr atr = new()
            {
                Val1 = 1,
                Val2 = 2
            };
            TestAtr atr2 = new()
            {
                Val1 = 1,
                Val2 = 2
            };

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithClassAttrTwoArrays",
                Params = [new TestAtr[] { atr, atr2 }, new TestAtr[] { atr }]
            };
            BasicDeliverEventArgs eventArgs = new("", 1, false, "", "", new BasicProperties(), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData)));

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.AreEqual(atr.Sum() + atr2.Sum() + atr.Sum(), (long)value);
        }
    }

    internal class TestRabbitMQController : RabbitMQControllerBase
    {
        public TestRabbitMQController()
        {

        }
        [RabbitMQMethod("TestTaskMethod")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task TestTaskMethod()
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            await Task.Delay(500);
        }
        [RabbitMQMethod("TestMethodWithResponse")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task<long> TestTaskMethodWithResponse()
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            await Task.Delay(600);
            return 0;
        }
        [RabbitMQMethod("TestMethodWithArguments")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task TestTaskMethodWithArguments(long a, long b)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            await Task.Delay(Convert.ToInt32(a));
            await Task.Delay(Convert.ToInt32(b));
        }
        [RabbitMQMethod("TestMethodWithArgumentsAndResponse")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<long> TestTaskMethodWithArgumentsAndResponse(long a, long b)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(a + b);
        }

        [RabbitMQMethod("TestMethodWithArgumentsAndResponseString")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<string> TestTaskMethodWithArgumentsAndResponseString(string a, string b)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(a + b);
        }
        [RabbitMQMethod("TestMethodWithClassAttr")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<long> TestTaskMethodWithArgumentsAndResponse(TestAtr atr)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(atr.Sum());
        }
        [RabbitMQMethod("TestMethodWithClassAttrArray")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<long> TestTaskMethodWithArgumentsAndResponseArray(TestAtr[] atrs)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(atrs.Sum(s => s.Sum()));
        }
        [RabbitMQMethod("TestMethodWithClassAttrTwoArrays")]
#pragma warning disable CA1822 // Пометьте члены как статические
        public Task<long> TestTaskMethodWithArgumentsAndResponseTwoArrays(TestAtr[] atrs, TestAtr[] atr)
#pragma warning restore CA1822 // Пометьте члены как статические
        {
            return Task.FromResult(atrs.Sum(s => s.Sum()) + atr.Sum(s => s.Sum()));
        }
    }

    public class TestAtr
    {
        public long Val1 { get; set; }
        public long Val2 { get; set; }

        public long Sum()
        {
            return Val1 + Val2;
        }
    }
}