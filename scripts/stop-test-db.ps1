# Stop Test Database Container
Write-Host "Stopping ErpBE Test Database..." -ForegroundColor Green

# Stop and remove container
docker-compose -f docker-compose.test.yml down

Write-Host "✅ Test database stopped!" -ForegroundColor Green
