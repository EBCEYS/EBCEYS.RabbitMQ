using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using Newtonsoft.Json;
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
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
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
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
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
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
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
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.IsTrue((long)value == 0);
        }
        [TestMethod]
        public async Task ProcessRequestWithArgumentsAsyncTest()
        {
            using TestRabbitMQController controller = new();

            long[] arr = { 100, 200 };
            var arrStr = JsonConvert.SerializeObject(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArguments",
                Params = JsonConvert.DeserializeObject<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            await controller.ProcessRequestAsync(method!);
        }
        [TestMethod]
        public async Task ProcessRequestWithArgumentsAndResponseAsyncTest()
        {
            using TestRabbitMQController controller = new();

            long[] arr = { 50, 200 };
            var arrStr = JsonConvert.SerializeObject(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArgumentsAndResponse",
                Params = JsonConvert.DeserializeObject<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.IsTrue((long)value == arr.Sum());
        }

        [TestMethod]
        public async Task ProcessRequestWithArgumentsAndResponseAsync_String_Test()
        {
            using TestRabbitMQController controller = new();

            string[] arr = { "50", "200" };
            var arrStr = JsonConvert.SerializeObject(arr);

            RabbitMQRequestData requestData = new()
            {
                Method = "TestMethodWithArgumentsAndResponseString",
                Params = JsonConvert.DeserializeObject<object[]>(arrStr)
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(string));

            Assert.IsNotNull(value);
            Assert.IsTrue((string)value == arr[0] + arr[1]);
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
                Params = new[] {atr}
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.IsTrue(((long)value) == atr.Sum());
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
                Params = new[] { new TestAtr[] { atr, atr2 } }
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.IsTrue(((long)value) == atr.Sum() + atr2.Sum());
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
                Params = new[] { new TestAtr[] { atr, atr2 }, new TestAtr[] { atr } }
            };
            BasicDeliverEventArgs eventArgs = new()
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData))
            };

            MethodInfo? method = controller.GetMethodToExecute(eventArgs);

            object? value = await controller.ProcessRequestWithResponseAsync(method!);

            value = Convert.ChangeType(value, typeof(long));

            Assert.IsNotNull(value);
            Assert.IsTrue(((long)value) == atr.Sum() + atr2.Sum() + atr.Sum());
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
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task<long> TestTaskMethodWithArgumentsAndResponse(long a, long b)
#pragma warning restore CA1822 // Пометьте члены как статические
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            return a + b;
        }

        [RabbitMQMethod("TestMethodWithArgumentsAndResponseString")]
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task<string> TestTaskMethodWithArgumentsAndResponseString(string a, string b)
#pragma warning restore CA1822 // Пометьте члены как статические
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            return a + b;
        }
        [RabbitMQMethod("TestMethodWithClassAttr")]
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task<long> TestTaskMethodWithArgumentsAndResponse(TestAtr atr)
#pragma warning restore CA1822 // Пометьте члены как статические
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            return atr.Sum();
        }
        [RabbitMQMethod("TestMethodWithClassAttrArray")]
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task<long> TestTaskMethodWithArgumentsAndResponseArray(TestAtr[] atrs)
#pragma warning restore CA1822 // Пометьте члены как статические
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            return atrs.Sum(s => s.Sum());
        }
        [RabbitMQMethod("TestMethodWithClassAttrTwoArrays")]
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
#pragma warning disable CA1822 // Пометьте члены как статические
        public async Task<long> TestTaskMethodWithArgumentsAndResponseTwoArrays(TestAtr[] atrs, TestAtr[] atr)
#pragma warning restore CA1822 // Пометьте члены как статические
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            return atrs.Sum(s => s.Sum()) + atr.Sum(s => s.Sum());
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