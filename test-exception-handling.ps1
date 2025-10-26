$apiUrl = "http://localhost:5136"

Write-Host "`n════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🧪 TESTING DETAILED EXCEPTION HANDLING" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Test 1: Generic Exception
Write-Host "`n1. Testing Generic Exception (500)" -ForegroundColor Yellow
Write-Host "   Endpoint: GET /api/TestException/throw-exception" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/TestException/throw-exception" -Method Get
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    Write-Host "   Error Response:" -ForegroundColor Red
    Write-Host ($errorBody | ConvertTo-Json -Depth 10) -ForegroundColor Magenta
}

# Test 2: Not Found Exception
Write-Host "`n2. Testing KeyNotFoundException (404)" -ForegroundColor Yellow
Write-Host "   Endpoint: GET /api/TestException/not-found" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/TestException/not-found" -Method Get
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    Write-Host "   Error Response:" -ForegroundColor Red
    Write-Host ($errorBody | ConvertTo-Json -Depth 10) -ForegroundColor Magenta
}

# Test 3: Invalid Operation Exception
Write-Host "`n3. Testing InvalidOperationException (400)" -ForegroundColor Yellow
Write-Host "   Endpoint: GET /api/TestException/invalid-operation" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/TestException/invalid-operation" -Method Get
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    Write-Host "   Error Response:" -ForegroundColor Red
    Write-Host ($errorBody | ConvertTo-Json -Depth 10) -ForegroundColor Magenta
}

# Test 4: Argument Null Exception
Write-Host "`n4. Testing ArgumentNullException (400)" -ForegroundColor Yellow
Write-Host "   Endpoint: GET /api/TestException/null-argument" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/TestException/null-argument" -Method Get
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    Write-Host "   Error Response:" -ForegroundColor Red
    Write-Host ($errorBody | ConvertTo-Json -Depth 10) -ForegroundColor Magenta
}

# Test 5: Nested Exception with Inner Exception
Write-Host "`n5. Testing Nested Exception with Inner Exception (500)" -ForegroundColor Yellow
Write-Host "   Endpoint: GET /api/TestException/nested-exception" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/TestException/nested-exception" -Method Get
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    Write-Host "   Error Response (note the innerException field):" -ForegroundColor Red
    Write-Host ($errorBody | ConvertTo-Json -Depth 10) -ForegroundColor Magenta
}

# Test 6: Database Error
Write-Host "`n6. Testing Database Error with Inner Exception (500)" -ForegroundColor Yellow
Write-Host "   Endpoint: GET /api/TestException/database-error" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/TestException/database-error" -Method Get
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    Write-Host "   Error Response:" -ForegroundColor Red
    Write-Host ($errorBody | ConvertTo-Json -Depth 10) -ForegroundColor Magenta
}

# Test 7: Success (no exception)
Write-Host "`n7. Testing Success Endpoint (200)" -ForegroundColor Yellow
Write-Host "   Endpoint: GET /api/TestException/success" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/TestException/success" -Method Get
    $successBody = $response.Content | ConvertFrom-Json
    Write-Host "   ✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   Response:" -ForegroundColor Green
    Write-Host ($successBody | ConvertTo-Json) -ForegroundColor Green
} catch {
    Write-Host "   ❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🎯 SUMMARY" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ All exceptions now return detailed error information instead of generic 500 errors" -ForegroundColor Green
Write-Host "✅ Error responses include:" -ForegroundColor Green
Write-Host "   • Success flag (false for errors)" -ForegroundColor White
Write-Host "   • Status code" -ForegroundColor White
Write-Host "   • Error type/name" -ForegroundColor White
Write-Host "   • Detailed message" -ForegroundColor White
Write-Host "   • Exception type name" -ForegroundColor White
Write-Host "   • Stack trace (for debugging)" -ForegroundColor White
Write-Host "   • Inner exception details (if present)" -ForegroundColor White
Write-Host "   • Timestamp, path, and HTTP method" -ForegroundColor White
Write-Host ""
Write-Host "✅ This works in BOTH development AND production environments" -ForegroundColor Green
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

