# Start Test Database Container
Write-Host "Starting ErpBE Test Database..." -ForegroundColor Green

# Check if container is already running
$container = docker ps -q -f name=erp-test-db
if ($container) {
    Write-Host "Container is already running!" -ForegroundColor Yellow
    exit 0
}

# Start the container
docker-compose -f docker-compose.test.yml up -d

# Wait for container to be ready
Write-Host "Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Test connection
Write-Host "Testing database connection..." -ForegroundColor Yellow
docker exec erp-test-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "TestPassword123!" -C -Q "SELECT 1" | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Test database is ready!" -ForegroundColor Green
    Write-Host "Connection String: Server=localhost,1434;Database=ErpBE_Test;User Id=sa;Password=TestPassword123!;TrustServerCertificate=true;" -ForegroundColor Cyan
} else {
    Write-Host "❌ Failed to connect to test database" -ForegroundColor Red
}
