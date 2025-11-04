# PowerShell script to deploy Company and FinancialYear stored procedures
# This script deploys:
# - ERP_GetActiveCompanies
# - ERP_GetFinancialYearsByCompanyId

$ErrorActionPreference = "Stop"

Write-Host "`n======================================" -ForegroundColor Cyan
Write-Host "Deploying Company and FinancialYear Stored Procedures" -ForegroundColor Cyan
Write-Host "======================================`n" -ForegroundColor Cyan

# Use production connection string
$connectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;"

Write-Host "Connection: SQL5111.site4now.net -> db_a2ea4b_sunv2`n" -ForegroundColor Gray

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath

# Stored procedure files
$companySP = Join-Path $rootPath "Database_Scripts\StoredProcedures\Company\ERP_GetActiveCompanies.sql"
$financialYearSP = Join-Path $rootPath "Database_Scripts\StoredProcedures\FinancialYear\ERP_GetFinancialYearsByCompanyId.sql"

# Check if files exist
if (-not (Test-Path $companySP)) {
    Write-Host "ERROR: Company stored procedure file not found: $companySP" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $financialYearSP)) {
    Write-Host "ERROR: FinancialYear stored procedure file not found: $financialYearSP" -ForegroundColor Red
    exit 1
}

$successCount = 0
$failCount = 0

# Deploy Company stored procedure
Write-Host "Deploying: ERP_GetActiveCompanies..." -ForegroundColor White -NoNewline
try {
    $sqlContent = Get-Content -Path $companySP -Raw
    
    # Split by GO statements and execute each batch
    $batches = $sqlContent -split '\r?\nGO\r?\n'
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    foreach ($batch in $batches) {
        $batch = $batch.Trim()
        if ($batch.Length -gt 0) {
            $command = $connection.CreateCommand()
            $command.CommandText = $batch
            $command.CommandTimeout = 60
            $command.ExecuteNonQuery() | Out-Null
        }
    }
    
    $connection.Close()
    $connection.Dispose()
    
    Write-Host " ✅ SUCCESS" -ForegroundColor Green
    $successCount++
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    $failCount++
}

# Deploy FinancialYear stored procedure
Write-Host "Deploying: ERP_GetFinancialYearsByCompanyId..." -ForegroundColor White -NoNewline
try {
    $sqlContent = Get-Content -Path $financialYearSP -Raw
    
    # Split by GO statements and execute each batch
    $batches = $sqlContent -split '\r?\nGO\r?\n'
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    foreach ($batch in $batches) {
        $batch = $batch.Trim()
        if ($batch.Length -gt 0) {
            $command = $connection.CreateCommand()
            $command.CommandText = $batch
            $command.CommandTimeout = 60
            $command.ExecuteNonQuery() | Out-Null
        }
    }
    
    $connection.Close()
    $connection.Dispose()
    
    Write-Host " ✅ SUCCESS" -ForegroundColor Green
    $successCount++
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    $failCount++
}

Write-Host "`n======================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Total: 2 | Success: $successCount | Failed: $failCount`n" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Yellow" })

if ($failCount -gt 0) {
    exit 1
}

