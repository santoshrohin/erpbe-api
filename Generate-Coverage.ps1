# Quick Code Coverage Generator
Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       GENERATING CODE COVERAGE REPORT              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Step 1: Run tests with coverage
Write-Host "⏳ Running tests with coverage..." -ForegroundColor Yellow
dotnet test ErpBE.Tests --collect:"XPlat Code Coverage" --logger:"console;verbosity=minimal"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Tests failed" -ForegroundColor Red
    exit 1
}

# Step 2: Find coverage file
$coverageFile = Get-ChildItem -Path "TestResults" -Filter "*.cobertura.xml" -Recurse | Select-Object -First 1

if (-not $coverageFile) {
    Write-Host "`n❌ No coverage file found" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Coverage file: $($coverageFile.FullName)`n" -ForegroundColor Green

# Step 3: Generate HTML report
Write-Host "⏳ Generating HTML report..." -ForegroundColor Yellow
reportgenerator "-reports:$($coverageFile.FullName)" "-targetdir:coverage-report" "-reporttypes:Html" "-verbosity:Warning"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ Failed to generate report" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Report generated!`n" -ForegroundColor Green

# Display summary
if (Test-Path "coverage-report\Summary.txt") {
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
    Write-Host "  COVERAGE SUMMARY" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray
    Get-Content "coverage-report\Summary.txt" | Select-String -Pattern "Line coverage|Branch coverage|Summary" | ForEach-Object {
        Write-Host "  $_" -ForegroundColor White
    }
}

Write-Host "`n📁 Report: coverage-report\index.html" -ForegroundColor Cyan
Write-Host "🌐 Opening in browser...`n" -ForegroundColor Yellow

Start-Process "coverage-report\index.html"

Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║            COVERAGE REPORT COMPLETE                ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

