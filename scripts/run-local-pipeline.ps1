# Run Local CI/CD Pipeline
param(
    [switch]$SkipTests = $false,
    [switch]$DeployToProduction = $false
)

Write-Host "Running Local CI/CD Pipeline..." -ForegroundColor Green

# Step 1: Clean and Restore
Write-Host "Step 1: Cleaning and Restoring..." -ForegroundColor Yellow
dotnet clean
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Restore failed!" -ForegroundColor Red
    exit 1
}

# Step 2: Build
Write-Host "Step 2: Building..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Step 3: Start Test Database
Write-Host "Step 3: Starting Test Database..." -ForegroundColor Yellow
.\scripts\start-test-db.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start test database!" -ForegroundColor Red
    exit 1
}

# Step 4: Run Tests
if (-not $SkipTests) {
    Write-Host "Step 4: Running Tests..." -ForegroundColor Yellow
    dotnet test --configuration Release --logger "console;verbosity=normal"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests failed!" -ForegroundColor Red
        Write-Host "Stopping test database..." -ForegroundColor Yellow
        .\scripts\stop-test-db.ps1
        exit 1
    }
} else {
    Write-Host "Step 4: Skipping Tests" -ForegroundColor Yellow
}

# Step 5: Deploy to Production (if requested)
if ($DeployToProduction) {
    Write-Host "Step 5: Deploying to Production..." -ForegroundColor Yellow
    
    # Dry run first
    Write-Host "Running dry run..." -ForegroundColor Cyan
    .\scripts\deploy-to-production.ps1 -DryRun
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Dry run failed!" -ForegroundColor Red
        exit 1
    }
    
    # Confirm deployment
    $confirm = Read-Host "Do you want to proceed with production deployment? (y/N)"
    if ($confirm -eq "y" -or $confirm -eq "Y") {
        .\scripts\deploy-to-production.ps1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Production deployment failed!" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "Production deployment completed!" -ForegroundColor Green
    } else {
        Write-Host "Production deployment cancelled" -ForegroundColor Yellow
    }
} else {
    Write-Host "Step 5: Skipping Production Deployment" -ForegroundColor Yellow
}

# Step 6: Cleanup
Write-Host "Step 6: Cleaning up..." -ForegroundColor Yellow
.\scripts\stop-test-db.ps1

Write-Host "Local CI/CD Pipeline completed successfully!" -ForegroundColor Green

# Summary
Write-Host "`nPipeline Summary:" -ForegroundColor Cyan
Write-Host "  Clean and Restore" -ForegroundColor Green
Write-Host "  Build" -ForegroundColor Green
Write-Host "  Test Database" -ForegroundColor Green
if (-not $SkipTests) {
    Write-Host "  Tests" -ForegroundColor Green
} else {
    Write-Host "  Tests (Skipped)" -ForegroundColor Yellow
}
if ($DeployToProduction) {
    Write-Host "  Production Deployment" -ForegroundColor Green
} else {
    Write-Host "  Production Deployment (Skipped)" -ForegroundColor Yellow
}
Write-Host "  Cleanup" -ForegroundColor Green