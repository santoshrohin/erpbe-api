<#
.SYNOPSIS
    Sets up the TestUser account for integration testing

.DESCRIPTION
    This script creates the TestUser account with Admin role in the production database.
    It should be run once before running integration tests.

.PARAMETER ServerName
    SQL Server instance name (default: SQL5111.site4now.net)

.PARAMETER DatabaseName
    Database name (default: db_a2ea4b_sunv2)

.PARAMETER Username
    SQL Server username (default: db_a2ea4b_sunv2_admin)

.PARAMETER Password
    SQL Server password (default: abcd@1234)

.EXAMPLE
    .\Setup-TestUser.ps1
    
.EXAMPLE
    .\Setup-TestUser.ps1 -ServerName "localhost\SQLEXPRESS" -DatabaseName "MyDB" -Username "sa" -Password "MyPass"
#>

param(
    [string]$ServerName = "SQL5111.site4now.net",
    [string]$DatabaseName = "db_a2ea4b_sunv2",
    [string]$Username = "db_a2ea4b_sunv2_admin",
    [string]$Password = "abcd@1234"
)

$ErrorActionPreference = "Stop"

Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       SETTING UP TEST USER FOR INTEGRATION         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Define paths
$ScriptRoot = Split-Path -Parent $PSScriptRoot
$SetupScriptPath = Join-Path $ScriptRoot "ErpBE.Infrastructure\Scripts\Setup_Test_User.sql"

# Check if setup script exists
if (-not (Test-Path $SetupScriptPath)) {
    Write-Host "❌ Error: Setup script not found at: $SetupScriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "📝 Script Location: $SetupScriptPath" -ForegroundColor Gray
Write-Host "🔧 Server: $ServerName" -ForegroundColor Gray
Write-Host "💾 Database: $DatabaseName`n" -ForegroundColor Gray

try {
    Write-Host "⏳ Creating TestUser account..." -ForegroundColor Yellow
    
    # Execute the SQL script
    $result = Get-Content $SetupScriptPath | sqlcmd -S $ServerName -d $DatabaseName -U $Username -P $Password -W 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n❌ Failed to create TestUser" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
    
    Write-Host $result
    
    Write-Host "`n✅ TestUser setup completed successfully!" -ForegroundColor Green
    Write-Host "`n📋 Test User Credentials:" -ForegroundColor Cyan
    Write-Host "   Username: TestUser" -ForegroundColor White
    Write-Host "   Password: Test@123" -ForegroundColor White
    Write-Host "   Company ID: 1" -ForegroundColor White
    Write-Host "   Financial Year Code: -2147483641" -ForegroundColor White
    Write-Host "   Role: Admin`n" -ForegroundColor White
    
    Write-Host "✨ You can now run integration tests!" -ForegroundColor Green
    Write-Host "   Command: dotnet test ErpBE.Tests`n" -ForegroundColor Gray
}
catch {
    Write-Host "`n❌ Error: $_" -ForegroundColor Red
    exit 1
}

