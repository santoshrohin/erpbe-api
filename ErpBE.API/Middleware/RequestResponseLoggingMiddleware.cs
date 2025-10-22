using System.Diagnostics;
using System.Text;

namespace ErpBE.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];

            // Log request
            await LogRequestAsync(context, requestId);

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request {RequestId} failed: {Method} {Path}", 
                    requestId, context.Request.Method, context.Request.Path);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Log response
                await LogResponseAsync(context, requestId, responseBodyStream, stopwatch.ElapsedMilliseconds);

                // Copy response back to original stream
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId)
        {
            var request = context.Request;
            var user = context.User;

            var requestInfo = new
            {
                RequestId = requestId,
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                ContentType = request.ContentType,
                ContentLength = request.ContentLength,
                User = new
                {
                    Id = user.FindFirst("sub")?.Value ?? "Anonymous",
                    Name = user.FindFirst("username")?.Value ?? "Anonymous",
                    Role = user.FindFirst("role")?.Value ?? "Unknown"
                },
                IpAddress = GetClientIpAddress(context),
                UserAgent = request.Headers.UserAgent.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Request {RequestId} started: {Method} {Path} by {User}", 
                requestId, request.Method, request.Path, requestInfo.User.Name);

            // Log request body for POST/PUT/PATCH
            if (request.Method is "POST" or "PUT" or "PATCH" && request.ContentLength > 0)
            {
                var body = await ReadRequestBodyAsync(request);
                if (!string.IsNullOrEmpty(body))
                {
                    _logger.LogDebug("Request {RequestId} body: {Body}", requestId, body);
                }
            }
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, 
            MemoryStream responseBodyStream, long durationMs)
        {
            var response = context.Response;

            // Read response body
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);

            var responseInfo = new
            {
                RequestId = requestId,
                StatusCode = response.StatusCode,
                ContentType = response.ContentType,
                ContentLength = responseBody.Length,
                DurationMs = durationMs,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Timestamp = DateTime.UtcNow
            };

            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            
            _logger.Log(logLevel, "Request {RequestId} completed: {StatusCode} in {DurationMs}ms", 
                requestId, response.StatusCode, durationMs);

            // Log response body for errors or debug level
            if (response.StatusCode >= 400 || _logger.IsEnabled(LogLevel.Debug))
            {
                if (!string.IsNullOrEmpty(responseBody))
                {
                    _logger.LogDebug("Response {RequestId} body: {Body}", requestId, responseBody);
                }
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength == 0 || !request.Body.CanSeek)
                return string.Empty;

            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return body;
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
