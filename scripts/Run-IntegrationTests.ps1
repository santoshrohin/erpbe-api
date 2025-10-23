<#
.SYNOPSIS
    Runs integration tests with automated setup and cleanup

.DESCRIPTION
    This script automates the entire integration testing workflow:
    1. Ensures TestUser exists
    2. Runs all integration tests
    3. Optionally cleans up test data after completion

.PARAMETER SetupTestUser
    If specified, creates/recreates the TestUser before running tests

.PARAMETER CleanupAfterTests
    If specified, cleans up test data after tests complete

.PARAMETER TestFilter
    Optional test filter (e.g., "FullyQualifiedName~UnitMaster")

.PARAMETER Verbosity
    Test logger verbosity: quiet, minimal, normal, detailed (default: minimal)

.EXAMPLE
    .\Run-IntegrationTests.ps1
    Runs all tests without setup or cleanup

.EXAMPLE
    .\Run-IntegrationTests.ps1 -SetupTestUser -CleanupAfterTests
    Full workflow: setup, test, cleanup

.EXAMPLE
    .\Run-IntegrationTests.ps1 -TestFilter "FullyQualifiedName~UnitMaster" -Verbosity detailed
    Run specific tests with detailed output
#>

param(
    [switch]$SetupTestUser,
    [switch]$CleanupAfterTests,
    [string]$TestFilter,
    [ValidateSet("quiet", "minimal", "normal", "detailed")]
    [string]$Verbosity = "minimal"
)

$ErrorActionPreference = "Stop"

Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║      INTEGRATION TEST AUTOMATION WORKFLOW          ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Magenta

$ScriptRoot = Split-Path -Parent $PSScriptRoot
$SetupScript = Join-Path $PSScriptRoot "Setup-TestUser.ps1"
$CleanupScript = Join-Path $PSScriptRoot "Cleanup-TestData.ps1"

# Step 1: Setup TestUser (if requested)
if ($SetupTestUser) {
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
    Write-Host "  STEP 1: SETTING UP TEST USER" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray
    
    if (Test-Path $SetupScript) {
        & $SetupScript
        if ($LASTEXITCODE -ne 0) {
            Write-Host "`n❌ TestUser setup failed. Aborting..." -ForegroundColor Red
            exit 1
        }
    }
    else {
        Write-Host "❌ Setup script not found: $SetupScript" -ForegroundColor Red
        exit 1
    }
}

# Step 2: Build Solution
Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "  STEP 2: BUILDING SOLUTION" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray

Push-Location $ScriptRoot
try {
    Write-Host "⏳ Building solution..." -ForegroundColor Yellow
    dotnet build --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n❌ Build failed. Aborting..." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Build successful`n" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Step 3: Run Tests
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "  STEP 3: RUNNING INTEGRATION TESTS" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray

Push-Location $ScriptRoot
try {
    $testCommand = "dotnet test ErpBE.Tests --logger `"console;verbosity=$Verbosity`" --no-build"
    
    if ($TestFilter) {
        $testCommand += " --filter `"$TestFilter`""
        Write-Host "🔍 Filter: $TestFilter" -ForegroundColor Gray
    }
    
    Write-Host "⏳ Running tests...`n" -ForegroundColor Yellow
    
    Invoke-Expression $testCommand
    $testExitCode = $LASTEXITCODE
    
    if ($testExitCode -eq 0) {
        Write-Host "`n✅ All tests passed!" -ForegroundColor Green
    }
    else {
        Write-Host "`n❌ Some tests failed" -ForegroundColor Red
    }
}
finally {
    Pop-Location
}

# Step 4: Cleanup (if requested)
if ($CleanupAfterTests) {
    Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
    Write-Host "  STEP 4: CLEANING UP TEST DATA" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray
    
    if (Test-Path $CleanupScript) {
        & $CleanupScript
        if ($LASTEXITCODE -ne 0) {
            Write-Host "`n⚠️  Cleanup failed, but tests completed" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "⚠️  Cleanup script not found: $CleanupScript" -ForegroundColor Yellow
    }
}

Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║             WORKFLOW COMPLETED                      ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Magenta

exit $testExitCode

