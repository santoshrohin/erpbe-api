# Deploy Database Changes to Production
param(
    [string]$ProductionConnectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=true;",
    [string]$DatabaseProjectPath = "ErpBE.Database",
    [switch]$DryRun = $false
)

Write-Host "🚀 Deploying Database Changes to Production..." -ForegroundColor Green

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
}

# Function to execute SQL
function Invoke-SqlCommand {
    param(
        [string]$ConnectionString,
        [string]$Query,
        [bool]$IsDryRun = $false
    )
    
    if ($IsDryRun) {
        Write-Host "  [DRY RUN] Would execute: $($Query.Substring(0, [Math]::Min(100, $Query.Length)))..." -ForegroundColor Cyan
        return
    }
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $command = New-Object System.Data.SqlClient.SqlCommand($Query, $connection)
    $connection.Open()
    
    try {
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "  ✅ Executed successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "  ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
    finally {
        $connection.Close()
    }
}

# Get all SQL files from database project
$sqlFiles = Get-ChildItem -Path $DatabaseProjectPath -Recurse -Filter "*.sql" | Where-Object { $_.Name -notlike "*test*" }

Write-Host "📋 Found $($sqlFiles.Count) SQL files to deploy" -ForegroundColor Yellow

foreach ($file in $sqlFiles) {
    Write-Host "📄 Processing: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content -Path $file.FullName -Raw
    
    # Split by GO statements
    $statements = $content -split "GO\s*" | Where-Object { $_.Trim() -ne "" }
    
    foreach ($statement in $statements) {
        $statement = $statement.Trim()
        if ($statement -ne "") {
            Write-Host "  Executing statement..." -ForegroundColor Gray
            Invoke-SqlCommand -ConnectionString $ProductionConnectionString -Query $statement -IsDryRun $DryRun
        }
    }
}

if ($DryRun) {
    Write-Host "🔍 Dry run completed. Use -DryRun:$false to execute changes." -ForegroundColor Yellow
} else {
    Write-Host "✅ Database deployment completed!" -ForegroundColor Green
    Write-Host "🔄 Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Verify changes in production" -ForegroundColor White
    Write-Host "  2. Run: .\scripts\sync-production-schema.ps1" -ForegroundColor White
    Write-Host "  3. Run: .\scripts\reset-test-db.ps1" -ForegroundColor White
    Write-Host "  4. Run tests to ensure everything works" -ForegroundColor White
}
