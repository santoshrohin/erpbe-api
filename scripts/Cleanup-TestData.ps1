<#
.SYNOPSIS
    Cleans up test data created during integration testing

.DESCRIPTION
    This script removes all test data created by TestUser during integration tests.
    Run this after tests complete or when cleaning up orphaned test data.

.PARAMETER ServerName
    SQL Server instance name (default: SQL5111.site4now.net)

.PARAMETER DatabaseName
    Database name (default: db_a2ea4b_sunv2)

.PARAMETER Username
    SQL Server username (default: db_a2ea4b_sunv2_admin)

.PARAMETER Password
    SQL Server password (default: abcd@1234)

.PARAMETER DeleteTestUser
    If specified, also deletes the TestUser account

.EXAMPLE
    .\Cleanup-TestData.ps1
    
.EXAMPLE
    .\Cleanup-TestData.ps1 -DeleteTestUser
    
.EXAMPLE
    .\Cleanup-TestData.ps1 -ServerName "localhost\SQLEXPRESS" -DatabaseName "MyDB"
#>

param(
    [string]$ServerName = "SQL5111.site4now.net",
    [string]$DatabaseName = "db_a2ea4b_sunv2",
    [string]$Username = "db_a2ea4b_sunv2_admin",
    [string]$Password = "abcd@1234",
    [switch]$DeleteTestUser
)

$ErrorActionPreference = "Stop"

Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║         CLEANING UP TEST DATA FROM DATABASE        ║" -ForegroundColor Yellow
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Yellow

# Define paths
$ScriptRoot = Split-Path -Parent $PSScriptRoot
$CleanupScriptPath = Join-Path $ScriptRoot "ErpBE.Infrastructure\Scripts\Cleanup_Test_Data.sql"

# Check if cleanup script exists
if (-not (Test-Path $CleanupScriptPath)) {
    Write-Host "❌ Error: Cleanup script not found at: $CleanupScriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "📝 Script Location: $CleanupScriptPath" -ForegroundColor Gray
Write-Host "🔧 Server: $ServerName" -ForegroundColor Gray
Write-Host "💾 Database: $DatabaseName`n" -ForegroundColor Gray

try {
    Write-Host "⏳ Cleaning up test data..." -ForegroundColor Yellow
    
    # Execute the cleanup script
    $result = Get-Content $CleanupScriptPath | sqlcmd -S $ServerName -d $DatabaseName -U $Username -P $Password -W 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n❌ Failed to cleanup test data" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
    
    Write-Host $result
    
    # Optionally delete TestUser
    if ($DeleteTestUser) {
        Write-Host "`n⚠️  Deleting TestUser account..." -ForegroundColor Yellow
        
        $deleteUserSql = @"
-- Get TestUser ID
DECLARE @TestUserId INT;
SELECT @TestUserId = UM_CODE FROM USER_MASTER WHERE UM_USERNAME = 'TestUser';

IF @TestUserId IS NOT NULL
BEGIN
    -- Delete user roles
    DELETE FROM UserRoles WHERE UserId = @TestUserId;
    
    -- Delete user
    DELETE FROM USER_MASTER WHERE UM_CODE = @TestUserId;
    
    PRINT 'TestUser account deleted successfully.';
END
ELSE
BEGIN
    PRINT 'TestUser account not found.';
END
"@
        
        $deleteResult = $deleteUserSql | sqlcmd -S $ServerName -d $DatabaseName -U $Username -P $Password -W 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "`n❌ Failed to delete TestUser" -ForegroundColor Red
            Write-Host $deleteResult -ForegroundColor Red
        }
        else {
            Write-Host $deleteResult
            Write-Host "✅ TestUser account deleted" -ForegroundColor Green
        }
    }
    
    Write-Host "`n✅ Test data cleanup completed successfully!" -ForegroundColor Green
    
    if (-not $DeleteTestUser) {
        Write-Host "`n📋 Note: TestUser account was NOT deleted." -ForegroundColor Cyan
        Write-Host "   To delete TestUser, run: .\Cleanup-TestData.ps1 -DeleteTestUser`n" -ForegroundColor Gray
    }
}
catch {
    Write-Host "`n❌ Error: $_" -ForegroundColor Red
    exit 1
}

