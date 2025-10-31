param(
    [string]$ScriptsPath = "Database_Scripts\StoredProcedures\CustomerPo"
)

Write-Host "`n======================================" -ForegroundColor Cyan
Write-Host "Deploying Customer PO Stored Procedures" -ForegroundColor Cyan
Write-Host "======================================`n" -ForegroundColor Cyan

# Use production connection string
$connectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;"

Write-Host "Connection: SQL5111.site4now.net -> db_a2ea4b_sunv2`n" -ForegroundColor Gray

# Get all SQL files in the CustomerPo directory
$sqlFiles = Get-ChildItem -Path $ScriptsPath -Filter "*.sql" | Sort-Object Name

if ($sqlFiles.Count -eq 0) {
    Write-Host "No SQL files found in $ScriptsPath" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found $($sqlFiles.Count) stored procedure(s) to deploy`n" -ForegroundColor Green

$successCount = 0
$failCount = 0

foreach ($file in $sqlFiles) {
    Write-Host "Deploying: $($file.Name)..." -ForegroundColor White -NoNewline
    
    try {
        $sqlContent = Get-Content -Path $file.FullName -Raw
        
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
}

Write-Host "`n======================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Total: $($sqlFiles.Count) | Success: $successCount | Failed: $failCount`n" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Yellow" })

if ($failCount -gt 0) {
    exit 1
}

