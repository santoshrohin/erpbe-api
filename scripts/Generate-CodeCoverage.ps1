<#
.SYNOPSIS
    Generates code coverage report for the entire solution

.DESCRIPTION
    This script runs all tests (unit + integration) and generates a comprehensive code coverage report.

.PARAMETER OpenReport
    If specified, automatically opens the HTML report in the default browser

.EXAMPLE
    .\Generate-CodeCoverage.ps1
    
.EXAMPLE
    .\Generate-CodeCoverage.ps1 -OpenReport
#>

param(
    [switch]$OpenReport
)

$ErrorActionPreference = "Stop"

Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       GENERATING CODE COVERAGE REPORT              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

$ScriptRoot = Split-Path -Parent $PSScriptRoot
$ReportPath = Join-Path $ScriptRoot "coverage-report"

# Clean up old report
if (Test-Path $ReportPath) {
    Remove-Item $ReportPath -Recurse -Force
    Write-Host "🧹 Cleaned up old report directory`n" -ForegroundColor Gray
}

# Clean up old TestResults
$TestResultsPath = Join-Path $ScriptRoot "TestResults"
if (Test-Path $TestResultsPath) {
    Remove-Item $TestResultsPath -Recurse -Force
}

# Step 1: Run tests with coverage
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "  STEP 1: RUNNING TESTS WITH COVERAGE COLLECTION" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray

Push-Location $ScriptRoot
try {
    Write-Host "⏳ Building and running tests..." -ForegroundColor Yellow
    
    $loggerArg = "console;verbosity=minimal"
    dotnet test ErpBE.Tests --collect:"XPlat Code Coverage" --logger:$loggerArg
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n❌ Tests failed. Coverage report may be incomplete." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "`n✅ Tests completed successfully`n" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Find coverage file
Push-Location $ScriptRoot
$coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue

if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage file found!" -ForegroundColor Red
    Pop-Location
    exit 1
}

$coverageFile = $coverageFiles[0].FullName
Write-Host "📊 Coverage file: $coverageFile`n" -ForegroundColor Gray
Pop-Location

# Step 2: Generate HTML Report
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "  STEP 2: GENERATING HTML REPORT" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray

Push-Location $ScriptRoot
try {
    Write-Host "⏳ Generating coverage report..." -ForegroundColor Yellow
    
    reportgenerator "-reports:$coverageFile" "-targetdir:$ReportPath" "-reporttypes:Html" "-verbosity:Warning"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n❌ Failed to generate report" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Report generated successfully`n" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Display Summary
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "  COVERAGE REPORT SUMMARY" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray

$summaryFile = Join-Path $ReportPath "Summary.txt"
if (Test-Path $summaryFile) {
    Get-Content $summaryFile | Where-Object { $_ -match "Line coverage|Branch coverage|Summary" } | ForEach-Object {
        Write-Host "  $_" -ForegroundColor White
    }
}

Write-Host "`n📁 Report Location:" -ForegroundColor Cyan
Write-Host "   $ReportPath`n" -ForegroundColor White

$indexFile = Join-Path $ReportPath "index.html"
if (Test-Path $indexFile) {
    Write-Host "🌐 HTML Report:" -ForegroundColor Cyan
    $reportUrl = "file:///" + $indexFile.Replace('\', '/')
    Write-Host "   $reportUrl`n" -ForegroundColor White
    
    if ($OpenReport) {
        Write-Host "🚀 Opening report in browser...`n" -ForegroundColor Yellow
        Start-Process $indexFile
    }
}

Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║          CODE COVERAGE REPORT COMPLETE             ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

if (-not $OpenReport) {
    Write-Host "💡 Tip: Use -OpenReport to automatically open the HTML report" -ForegroundColor Gray
    Write-Host "   Example: .\scripts\Generate-CodeCoverage.ps1 -OpenReport`n" -ForegroundColor Gray
}
