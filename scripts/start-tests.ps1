# Start Tests with Container Database
param(
    [switch]$StopAfterTests = $true
)

Write-Host "Starting Tests with Container Database..." -ForegroundColor Green

# Step 1: Start Test Database Container
Write-Host "Step 1: Starting Test Database Container..." -ForegroundColor Yellow
.\scripts\start-test-db.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start test database container!" -ForegroundColor Red
    exit 1
}

# Step 2: Wait for database to be ready
Write-Host "Step 2: Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Step 3: Run Tests
Write-Host "Step 3: Running Tests..." -ForegroundColor Yellow
dotnet test --configuration Release --logger "console;verbosity=normal"

# Step 4: Stop container if requested
if ($StopAfterTests) {
    Write-Host "Step 4: Stopping Test Database Container..." -ForegroundColor Yellow
    .\scripts\stop-test-db.ps1
}

Write-Host "Test execution completed!" -ForegroundColor Green
