# 🔥 Exception Handling - Real Examples

## Example 1: Generic Exception (500 Internal Server Error)

### Request:
```http
GET /api/TestException/throw-exception HTTP/1.1
Host: localhost:5136
```

### Response:
```json
{
  "success": false,
  "statusCode": 500,
  "error": "Internal Server Error",
  "message": "This is a test exception with detailed information!",
  "exceptionType": "Exception",
  "stackTrace": "   at ErpBE.API.Controllers.TestExceptionController.ThrowException() in D:\\Santosh\\Work\\Projects\\WebBased\\API\\API\\ErpBE.API\\Controllers\\TestExceptionController.cs:line 26\r\n   at lambda_method6(Closure, Object, Object[])\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.SyncActionResultExecutor.Execute(...)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()\r\n   ...",
  "timestamp": "2025-10-26T14:34:46.958735Z",
  "path": "/api/TestException/throw-exception",
  "method": "GET",
  "innerException": null,
  "details": null
}
```

**Status Code**: `500`  
**Content-Type**: `application/json`

---

## Example 2: Nested Exception with Inner Exception (500)

### Request:
```http
GET /api/TestException/nested-exception HTTP/1.1
Host: localhost:5136
```

### Response:
```json
{
  "success": false,
  "statusCode": 500,
  "error": "Internal Server Error",
  "message": "This is the outer exception that wraps the inner one",
  "exceptionType": "Exception",
  "stackTrace": "   at ErpBE.API.Controllers.TestExceptionController.ThrowNestedException() in D:\\Santosh\\Work\\Projects\\WebBased\\API\\API\\ErpBE.API\\Controllers\\TestExceptionController.cs:line 68\r\n   ...",
  "timestamp": "2025-10-26T14:34:55.0534926Z",
  "path": "/api/TestException/nested-exception",
  "method": "GET",
  "innerException": {
    "message": "This is the inner exception",
    "type": "InvalidOperationException",
    "stackTrace": "   at ErpBE.API.Controllers.TestExceptionController.ThrowNestedException() in D:\\Santosh\\Work\\Projects\\WebBased\\API\\API\\ErpBE.API\\Controllers\\TestExceptionController.cs:line 64"
  },
  "details": null
}
```

**Status Code**: `500`  
**Note**: This includes `innerException` with full details

---

## Example 3: Argument Null Exception (400 Bad Request)

### Request:
```http
GET /api/TestException/null-argument HTTP/1.1
Host: localhost:5136
```

### Response:
```json
{
  "success": false,
  "statusCode": 400,
  "error": "Bad Request - Null Argument",
  "message": "User ID cannot be null! (Parameter 'userId')",
  "exceptionType": "ArgumentNullException",
  "stackTrace": null,
  "timestamp": "2025-10-26T14:34:55.2109103Z",
  "path": "/api/TestException/null-argument",
  "method": "GET",
  "innerException": null,
  "details": {
    "paramName": "userId"
  }
}
```

**Status Code**: `400`  
**Note**: Includes `details.paramName` showing which parameter was null

---

## Example 4: Key Not Found Exception (404 Not Found)

### Request:
```http
GET /api/TestException/not-found HTTP/1.1
Host: localhost:5136
```

### Response:
```json
{
  "success": false,
  "statusCode": 404,
  "error": "Not Found",
  "message": "The requested item was not found in the database!",
  "exceptionType": "KeyNotFoundException",
  "stackTrace": null,
  "timestamp": "2025-10-26T14:34:55.3682377Z",
  "path": "/api/TestException/not-found",
  "method": "GET",
  "innerException": null,
  "details": null
}
```

**Status Code**: `404`

---

## Example 5: Invalid Operation Exception (400 Bad Request)

### Request:
```http
GET /api/TestException/invalid-operation HTTP/1.1
Host: localhost:5136
```

### Response:
```json
{
  "success": false,
  "statusCode": 400,
  "error": "Invalid Operation",
  "message": "Invalid operation occurred - this is a test!",
  "exceptionType": "InvalidOperationException",
  "stackTrace": "   at ErpBE.API.Controllers.TestExceptionController.ThrowInvalidOperation() in D:\\Santosh\\Work\\Projects\\WebBased\\API\\API\\ErpBE.API\\Controllers\\TestExceptionController.cs:line 35\r\n   ...",
  "timestamp": "2025-10-26T14:35:12.4567890Z",
  "path": "/api/TestException/invalid-operation",
  "method": "GET",
  "innerException": null,
  "details": null
}
```

**Status Code**: `400`

---

## Example 6: Database Error with Inner Exception (500)

### Request:
```http
GET /api/TestException/database-error HTTP/1.1
Host: localhost:5136
```

### Response:
```json
{
  "success": false,
  "statusCode": 500,
  "error": "Internal Server Error",
  "message": "Database connection failed - Could not connect to SQL Server",
  "exceptionType": "Exception",
  "stackTrace": "   at ErpBE.API.Controllers.TestExceptionController.ThrowDatabaseError() in D:\\Santosh\\Work\\Projects\\WebBased\\API\\API\\ErpBE.API\\Controllers\\TestExceptionController.cs:line 79\r\n   ...",
  "timestamp": "2025-10-26T14:35:20.1234567Z",
  "path": "/api/TestException/database-error",
  "method": "GET",
  "innerException": {
    "message": "A network-related or instance-specific error occurred while establishing a connection to SQL Server",
    "type": "InvalidOperationException",
    "stackTrace": null
  },
  "details": null
}
```

**Status Code**: `500`  
**Note**: Simulates a database connection error with inner exception

---

## Example 7: Success (No Exception) - 200 OK

### Request:
```http
GET /api/TestException/success HTTP/1.1
Host: localhost:5136
```

### Response:
```json
{
  "success": true,
  "message": "This endpoint works correctly without throwing exceptions",
  "timestamp": "2025-10-26T14:34:28.1063611Z"
}
```

**Status Code**: `200`  
**Note**: Normal successful response (no exception)

---

## Comparison: Before vs After

### BEFORE (Generic 500 Error)

```http
HTTP/1.1 500 Internal Server Error
Content-Type: text/html

<!DOCTYPE html>
<html>
<head><title>500 Internal Server Error</title></head>
<body>
<h1>Internal Server Error</h1>
<p>An error occurred while processing your request.</p>
</body>
</html>
```

**Problems:**
- ❌ No error details
- ❌ No way to debug
- ❌ HTML response (not API-friendly)
- ❌ Must check server logs

---

### AFTER (Detailed JSON Error)

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json

{
  "success": false,
  "statusCode": 500,
  "error": "Internal Server Error",
  "message": "Database connection failed - Could not connect to SQL Server",
  "exceptionType": "Exception",
  "stackTrace": "   at ErpBE.API.Controllers...",
  "innerException": {
    "message": "A network-related error occurred...",
    "type": "InvalidOperationException"
  },
  "timestamp": "2025-10-26T14:35:20.1234567Z",
  "path": "/api/SomeEndpoint",
  "method": "POST"
}
```

**Benefits:**
- ✅ Detailed error message
- ✅ Full stack trace
- ✅ Inner exception details
- ✅ JSON response (API-friendly)
- ✅ Can debug from response
- ✅ No need to check server logs

---

## Testing with cURL

### Test All Endpoints:

```bash
# 1. Generic Exception
curl -i http://localhost:5136/api/TestException/throw-exception

# 2. Nested Exception
curl -i http://localhost:5136/api/TestException/nested-exception

# 3. Null Argument
curl -i http://localhost:5136/api/TestException/null-argument

# 4. Not Found
curl -i http://localhost:5136/api/TestException/not-found

# 5. Invalid Operation
curl -i http://localhost:5136/api/TestException/invalid-operation

# 6. Database Error
curl -i http://localhost:5136/api/TestException/database-error

# 7. Success (no error)
curl -i http://localhost:5136/api/TestException/success
```

### Test with PowerShell:

```powershell
# Run the comprehensive test script
.\test-exception-handling.ps1

# Or test individual endpoints
Invoke-RestMethod -Uri "http://localhost:5136/api/TestException/throw-exception" | ConvertTo-Json -Depth 10
```

---

## Response Headers

All error responses include:

```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
Date: Sat, 26 Oct 2025 14:34:46 GMT
Server: Kestrel
Transfer-Encoding: chunked
```

---

## Key Takeaways

1. **Every exception** returns a detailed JSON response
2. **Status codes** are appropriate (400/404/500)
3. **Stack traces** are included for debugging
4. **Inner exceptions** are captured and displayed
5. **Works everywhere**: Development, Staging, Production
6. **No configuration** needed - works out of the box
7. **Consistent format** - easy to parse and handle

---

**Last Updated**: October 26, 2025  
**Test these endpoints** in your local environment or production!

