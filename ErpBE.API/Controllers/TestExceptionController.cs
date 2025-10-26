using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers
{
    /// <summary>
    /// Test controller to demonstrate detailed exception handling
    /// This can be removed in production or kept for testing purposes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestExceptionController : ControllerBase
    {
        private readonly ILogger<TestExceptionController> _logger;

        public TestExceptionController(ILogger<TestExceptionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint that throws a generic exception
        /// </summary>
        [HttpGet("throw-exception")]
        public IActionResult ThrowException()
        {
            throw new Exception("This is a test exception with detailed information!");
        }

        /// <summary>
        /// Test endpoint that throws an InvalidOperationException
        /// </summary>
        [HttpGet("invalid-operation")]
        public IActionResult ThrowInvalidOperation()
        {
            throw new InvalidOperationException("Invalid operation occurred - this is a test!");
        }

        /// <summary>
        /// Test endpoint that throws a KeyNotFoundException
        /// </summary>
        [HttpGet("not-found")]
        public IActionResult ThrowNotFound()
        {
            throw new KeyNotFoundException("The requested item was not found in the database!");
        }

        /// <summary>
        /// Test endpoint that throws an ArgumentNullException
        /// </summary>
        [HttpGet("null-argument")]
        public IActionResult ThrowNullArgument()
        {
            throw new ArgumentNullException("userId", "User ID cannot be null!");
        }

        /// <summary>
        /// Test endpoint that throws a nested exception with inner exception
        /// </summary>
        [HttpGet("nested-exception")]
        public IActionResult ThrowNestedException()
        {
            try
            {
                throw new InvalidOperationException("This is the inner exception");
            }
            catch (Exception inner)
            {
                throw new Exception("This is the outer exception that wraps the inner one", inner);
            }
        }

        /// <summary>
        /// Test endpoint that simulates a database connection error
        /// </summary>
        [HttpGet("database-error")]
        public IActionResult ThrowDatabaseError()
        {
            var innerException = new InvalidOperationException("A network-related or instance-specific error occurred while establishing a connection to SQL Server");
            throw new Exception("Database connection failed - Could not connect to SQL Server", innerException);
        }

        /// <summary>
        /// Test endpoint that works correctly (no exception)
        /// </summary>
        [HttpGet("success")]
        public IActionResult Success()
        {
            return Ok(new
            {
                success = true,
                message = "This endpoint works correctly without throwing exceptions",
                timestamp = DateTime.UtcNow
            });
        }
    }
}

