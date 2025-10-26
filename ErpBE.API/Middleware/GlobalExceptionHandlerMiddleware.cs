using System.Net;
using System.Text.Json;
using FluentValidation;

namespace ErpBE.API.Middleware
{
    /// <summary>
    /// Global exception handler middleware that returns detailed exception information
    /// instead of generic 500 errors. This is enabled for both development and production
    /// to help with debugging and troubleshooting.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next, 
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Success = false,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                Method = context.Request.Method
            };

            // Handle specific exception types
            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Error = "Validation Error";
                    errorResponse.Message = "One or more validation errors occurred";
                    errorResponse.Details = validationEx.Errors.Select(e => new ValidationError
                    {
                        PropertyName = e.PropertyName,
                        ErrorMessage = e.ErrorMessage,
                        AttemptedValue = e.AttemptedValue
                    }).ToList();
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Error = "Unauthorized";
                    errorResponse.Message = exception.Message;
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Error = "Not Found";
                    errorResponse.Message = exception.Message;
                    break;

                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Error = "Invalid Operation";
                    errorResponse.Message = exception.Message;
                    errorResponse.ExceptionType = exception.GetType().Name;
                    errorResponse.StackTrace = exception.StackTrace;
                    break;

                case ArgumentNullException argNullEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Error = "Bad Request - Null Argument";
                    errorResponse.Message = argNullEx.Message;
                    errorResponse.ExceptionType = argNullEx.GetType().Name;
                    errorResponse.Details = new { ParamName = argNullEx.ParamName };
                    break;

                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Error = "Bad Request - Invalid Argument";
                    errorResponse.Message = argEx.Message;
                    errorResponse.ExceptionType = argEx.GetType().Name;
                    errorResponse.Details = new { ParamName = argEx.ParamName };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Error = "Internal Server Error";
                    errorResponse.Message = exception.Message;
                    errorResponse.ExceptionType = exception.GetType().Name;
                    errorResponse.StackTrace = exception.StackTrace;
                    
                    // Include inner exception details if present
                    if (exception.InnerException != null)
                    {
                        errorResponse.InnerException = new InnerExceptionDetails
                        {
                            Message = exception.InnerException.Message,
                            Type = exception.InnerException.GetType().Name,
                            StackTrace = exception.InnerException.StackTrace
                        };
                    }
                    break;
            }

            // Add status code to response
            errorResponse.StatusCode = response.StatusCode;

            // Serialize and write response
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true // Make it readable
            };

            var result = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await response.WriteAsync(result);
        }
    }

    /// <summary>
    /// Detailed error response with exception information
    /// </summary>
    public class ErrorResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public InnerExceptionDetails? InnerException { get; set; }
        public object? Details { get; set; } // For validation errors or additional info
    }

    /// <summary>
    /// Inner exception details
    /// </summary>
    public class InnerExceptionDetails
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
    }

    /// <summary>
    /// Validation error details
    /// </summary>
    public class ValidationError
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public object? AttemptedValue { get; set; }
    }
}

