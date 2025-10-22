using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;

namespace ErpBE.API.Controllers.Test
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestLoggingController : ControllerBase
    {
        private readonly ILogger<TestLoggingController> _logger;

        public TestLoggingController(ILogger<TestLoggingController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Test logging functionality
        /// </summary>
        /// <returns>Test result</returns>
        [HttpGet]
        public IActionResult TestLogs()
        {
            try
            {
                // Test different log levels with explicit level specification
                _logger.LogInformation("Test Information log - API accessed at {Timestamp}", DateTime.Now);
                _logger.LogWarning("Test Warning log - This is a test warning");
                _logger.LogError("Test Error log - This is a test error (not a real error)");
                _logger.LogDebug("Test Debug log - Debug information");

                // Test with custom properties
                using (LogContext.PushProperty("UserId", "TEST_USER"))
                using (LogContext.PushProperty("RequestId", "TEST-REQ-001"))
                using (LogContext.PushProperty("ActionName", "TestLogs"))
                {
                    _logger.LogInformation("Test log with custom properties - Level should be Information");
                }

                // Test Serilog directly
                Log.Information("Direct Serilog Information log - {Timestamp}", DateTime.Now);
                Log.Warning("Direct Serilog Warning log");
                Log.Error("Direct Serilog Error log");

                return Ok(new
                {
                    message = "Test logs generated successfully!",
                    timestamp = DateTime.Now,
                    instructions = "Check your database Logs table to see the entries with proper Level values"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while testing logs");
                return StatusCode(500, new { message = "Error testing logs", error = ex.Message });
            }
        }

        /// <summary>
        /// Generate multiple test logs
        /// </summary>
        /// <param name="count">Number of logs to generate</param>
        /// <returns>Test result</returns>
        [HttpPost("generate/{count}")]
        public IActionResult GenerateTestLogs(int count = 10)
        {
            try
            {
                for (int i = 1; i <= count; i++)
                {
                    using (LogContext.PushProperty("UserId", $"TEST_USER_{i}"))
                    using (LogContext.PushProperty("RequestId", $"TEST-REQ-{i:D3}"))
                    using (LogContext.PushProperty("ActionName", "GenerateTestLogs"))
                    {
                        _logger.LogInformation("Test log entry {Index} of {Total} - Generated at {Timestamp}", 
                            i, count, DateTime.Now);
                    }
                }

                return Ok(new
                {
                    message = $"Generated {count} test logs successfully!",
                    timestamp = DateTime.Now,
                    count = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating test logs");
                return StatusCode(500, new { message = "Error generating test logs", error = ex.Message });
            }
        }
    }
}
