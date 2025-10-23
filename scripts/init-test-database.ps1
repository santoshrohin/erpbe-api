# Initialize Test Database with Schema and Test Data
Write-Host "Initializing test database..." -ForegroundColor Green

# Copy scripts to container
Write-Host "Copying scripts to container..." -ForegroundColor Yellow
docker cp test-scripts/01-init-database.sql erp-test-db:/tmp/
docker cp test-scripts/02-create-procedures.sql erp-test-db:/tmp/
docker cp test-scripts/03-seed-test-data.sql erp-test-db:/tmp/

# Run init script
Write-Host "Creating tables and schema..." -ForegroundColor Yellow
docker exec erp-test-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "TestPassword123!" -d ErpBE_Test -C -i /tmp/01-init-database.sql

# Run procedures script
Write-Host "Creating stored procedures..." -ForegroundColor Yellow
docker exec erp-test-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "TestPassword123!" -d ErpBE_Test -C -i /tmp/02-create-procedures.sql

# Run seed data script
Write-Host "Seeding test data..." -ForegroundColor Yellow
docker exec erp-test-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "TestPassword123!" -d ErpBE_Test -C -i /tmp/03-seed-test-data.sql

Write-Host "✅ Test database initialized!" -ForegroundColor Green

