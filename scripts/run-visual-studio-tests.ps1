# Run Tests from Visual Studio with Container Database
Write-Host "Setting up Visual Studio Test Environment..." -ForegroundColor Green

# Step 1: Start Test Database Container
Write-Host "Step 1: Starting Test Database Container..." -ForegroundColor Yellow
.\scripts\start-test-db.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start test database container!" -ForegroundColor Red
    Write-Host "Make sure Docker is running and try again." -ForegroundColor Red
    exit 1
}

# Step 2: Wait for database to be ready
Write-Host "Step 2: Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Step 3: Verify database connection
Write-Host "Step 3: Verifying database connection..." -ForegroundColor Yellow
$testConnection = sqlcmd -S localhost,1434 -U sa -P "TestPassword123!" -C -Q "SELECT 1" -h -1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Database connection failed! Container may not be ready yet." -ForegroundColor Red
    Write-Host "Please wait a moment and try running your tests again." -ForegroundColor Yellow
    exit 1
}

Write-Host "Test environment is ready!" -ForegroundColor Green
Write-Host "You can now run tests from Visual Studio Test menu." -ForegroundColor Cyan
Write-Host "Tests will use the container database, not production." -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop the test database when done:" -ForegroundColor Yellow
Write-Host "  .\scripts\stop-test-db.ps1" -ForegroundColor Gray
