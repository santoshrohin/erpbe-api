# Reset Test Database
Write-Host "Resetting ErpBE Test Database..." -ForegroundColor Green

# Stop container
docker-compose -f docker-compose.test.yml down

# Remove volumes to ensure clean state
docker volume prune -f

# Start container
docker-compose -f docker-compose.test.yml up -d

# Wait for container to be ready
Write-Host "Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Run initialization scripts
Write-Host "Initializing database..." -ForegroundColor Yellow
docker exec erp-test-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "TestPassword123!" -C -i /docker-entrypoint-initdb.d/01-init-database.sql | Out-Null
docker exec erp-test-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "TestPassword123!" -C -i /docker-entrypoint-initdb.d/02-create-procedures.sql | Out-Null
docker exec erp-test-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "TestPassword123!" -C -i /docker-entrypoint-initdb.d/03-seed-test-data.sql | Out-Null

Write-Host "✅ Test database reset and ready!" -ForegroundColor Green
