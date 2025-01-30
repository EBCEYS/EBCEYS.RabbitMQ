using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Server.MappedService.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EBCEYS.RabbitMQ.ExampleDockerClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitMQController(ILogger<RabbitMQController> logger, RabbitMQClient client) : ControllerBase
    {
        [HttpPost("message")]
        public async Task<IActionResult> SendTestMessage()
        {
            try
            {
                await client.SendMessageAsync(new()
                {
                    Method = "TestMethodMessage",
                    Params = [1, 2]
                }, false, HttpContext.RequestAborted);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on sending rabbitmq message!");
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("messages/{count}")]
        public async Task<IActionResult> SendTestMessages([Required][FromRoute] uint count)
        {
            if (count <= 0)
            {
                return BadRequest("Count should be more than 0!");
            }
            try
            {
                List<Task> tasks = [];
                for (int i = 0; i < count; i++)
                {
                    tasks.Add(client.SendMessageAsync(new()
                    {
                        Method = "TestMethodMessage",
                        Params = [i, i + 1]
                    }, false, HttpContext.RequestAborted));
                }
                await Task.WhenAll(tasks);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on sending multiple messages!");
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("request")]
        public async Task<IActionResult> SendTestRequest([Required][FromQuery] long a, [Required][FromQuery] long b)
        {
            try
            {
                int result = await client.SendRequestAsync<int>(new()
                {
                    Method = "TestMethodRequest",
                    Params = [a, b]
                }, false, HttpContext.RequestAborted);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on sending rabbitmq request!");
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("requestwithmanyparams")]
        public async Task<IActionResult> SendTestRequestWithManyParams()
        {
            try
            {
                int result = await client.SendRequestAsync<int>(new()
                {
                    Method = "TestMethodRequest",
                    Params = [1, 2, 3]
                }, false, HttpContext.RequestAborted);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on sending rabbitmq request!");
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("requests/{count}")]
        public async Task<IActionResult> SendTestRequests([Required][FromRoute] uint count)
        {
            if (count <= 0)
            {
                return BadRequest("Count should be more than 0!");
            }
            try
            {
                List<Task<int>> tasks = [];
                for (int i = 0; i < count; i++)
                {
                    tasks.Add(client.SendRequestAsync<int>(new()
                    {
                        Method = "TestMethodRequest",
                        Params = [i, i + 1]
                    }, false, HttpContext.RequestAborted));
                }
                int[] results = await Task.WhenAll(tasks);
                return Ok(results);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on sending multiple requests!");
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("requests/without/response/{count}")]
        public async Task<IActionResult> SendTestRequestsWithoutResponse([Required][FromRoute] ulong count)
        {
            if (count <= 0)
            {
                return BadRequest("Count should be more than 0!");
            }
            try
            {
                List<Task<long>> tasks = [];
                for (ulong i = 0; i < count; i++)
                {
                    tasks.Add(client.SendRequestAsync<long>(new()
                    {
                        Method = "TestMethodRequest",
                        Params = [i, i + 1]
                    }, false, HttpContext.RequestAborted));
                }
                await Task.WhenAll(tasks);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on sending multiple requests!");
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("requests/jtoken")]
        public async Task<IActionResult> SendTestRequestWithJToken([Required][FromBody] JsonDocument someObject)
        {
            try
            {
                JToken token = JToken.Parse(someObject.RootElement.ToString());
                token["TestValue"] = 1;
                JToken? response = await client.SendRequestAsync<JToken>(new()
                {
                    Method = "TestMethodRequestJToken",
                    Params = [token]
                }, false, HttpContext.RequestAborted);
                return Ok(response?.ToString() ?? "empty response");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on sending request with jtoken");
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("requests/exception")]
        public async Task<IActionResult> SendTestRequestWithException([Required][FromQuery] string message)
        {
            try
            {
                object? response = await client.SendRequestAsync<object?>(new()
                {
                    Method = "TestMethodException",
                    Params = [message]
                });
                return Ok(response);
            }
            catch (RabbitMQRequestProcessingException ex)
            {
                return Ok(ex.ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("requests/with/innerexception")]
        public async Task<IActionResult> SendTestRequestWithInnerException([Required][FromQuery] string message)
        {
            try
            {
                object? response = await client.SendRequestAsync<object?>(new()
                {
                    Method = "TestMethodWithInnerException",
                    Params = [message]
                });
                return Ok(response);
            }
            catch (RabbitMQRequestProcessingException ex)
            {
                return Ok(ex.ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost("requests/with/unexpectedexception")]
        public async Task<IActionResult> SendTestRequestWithUnexpectedException([Required][FromQuery] string message)
        {
            try
            {
                object? response = await client.SendRequestAsync<object?>(new()
                {
                    Method = "TestMethodWithUnexpectedException",
                    Params = [message]
                });
                return Ok(response);
            }
            catch (RabbitMQRequestProcessingException ex)
            {
                return Ok(ex.ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
