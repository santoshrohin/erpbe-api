# ========================================
# Deploy to SmarterASP.NET
# ========================================

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "🚀 SmarterASP.NET Deployment Script" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan

# Configuration
$publishFolder = "D:\Santosh\Work\Publish\SmarterASP"
$ftpServer = "ftp://santoshrohini-001-site44.qtempurl.com"
$ftpUsername = "santoshrohini-001-site44"
$ftpPassword = Read-Host "Enter FTP Password" -AsSecureString
$ftpPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($ftpPassword))

# Step 1: Clean previous build
Write-Host "`n📦 Step 1: Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $publishFolder) {
    Remove-Item -Path $publishFolder -Recurse -Force
    Write-Host "   ✅ Cleaned: $publishFolder" -ForegroundColor Green
}

# Step 2: Restore NuGet packages
Write-Host "`n📦 Step 2: Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore ErpBE.API/ErpBE.API.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ❌ Failed to restore packages" -ForegroundColor Red
    exit 1
}
Write-Host "   ✅ Packages restored" -ForegroundColor Green

# Step 3: Build in Release mode
Write-Host "`n🔨 Step 3: Building in Release mode (.NET 8.0)..." -ForegroundColor Yellow
dotnet build ErpBE.API/ErpBE.API.csproj -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "   ❌ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "   ✅ Build successful" -ForegroundColor Green

# Step 4: Publish
Write-Host "`n📤 Step 4: Publishing application..." -ForegroundColor Yellow
dotnet publish ErpBE.API/ErpBE.API.csproj `
    -c Release `
    -o $publishFolder `
    --framework net8.0 `
    --runtime win-x64 `
    --self-contained false `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "   ❌ Publish failed" -ForegroundColor Red
    exit 1
}
Write-Host "   ✅ Published to: $publishFolder" -ForegroundColor Green

# Step 5: Create logs directory
Write-Host "`n📁 Step 5: Creating logs directory..." -ForegroundColor Yellow
$logsPath = Join-Path $publishFolder "logs"
if (!(Test-Path $logsPath)) {
    New-Item -Path $logsPath -ItemType Directory | Out-Null
}
Write-Host "   ✅ Logs directory created" -ForegroundColor Green

# Step 6: Verify critical files
Write-Host "`n🔍 Step 6: Verifying critical files..." -ForegroundColor Yellow
$criticalFiles = @(
    "ErpBE.API.dll",
    "web.config",
    "appsettings.json",
    "appsettings.Production.json"
)

$allFilesExist = $true
foreach ($file in $criticalFiles) {
    $filePath = Join-Path $publishFolder $file
    if (Test-Path $filePath) {
        Write-Host "   ✅ Found: $file" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Missing: $file" -ForegroundColor Red
        $allFilesExist = $false
    }
}

if (!$allFilesExist) {
    Write-Host "`n❌ Some critical files are missing. Aborting deployment." -ForegroundColor Red
    exit 1
}

# Step 7: Display publish summary
Write-Host "`n📊 Publish Summary:" -ForegroundColor Cyan
$publishedFiles = Get-ChildItem -Path $publishFolder -Recurse -File
$totalSize = ($publishedFiles | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "   Total Files: $($publishedFiles.Count)" -ForegroundColor White
Write-Host "   Total Size: $([math]::Round($totalSize, 2)) MB" -ForegroundColor White

# Step 8: FTP Upload Option
Write-Host "`n📤 Step 8: Upload to SmarterASP.NET via FTP" -ForegroundColor Yellow
Write-Host "   FTP Server: $ftpServer" -ForegroundColor White
Write-Host "   Username: $ftpUsername" -ForegroundColor White

$uploadChoice = Read-Host "`n   Do you want to upload via FTP now? (Y/N)"

if ($uploadChoice -eq "Y" -or $uploadChoice -eq "y") {
    Write-Host "`n   🚀 Starting FTP upload..." -ForegroundColor Yellow
    Write-Host "   ⚠️  This may take several minutes..." -ForegroundColor Yellow
    
    # Note: For large deployments, consider using WinSCP or FileZilla
    Write-Host "`n   ℹ️  For optimal upload speed, use FileZilla:" -ForegroundColor Cyan
    Write-Host "      1. Open FileZilla" -ForegroundColor White
    Write-Host "      2. Host: ftp://santoshrohini-001-site44.qtempurl.com" -ForegroundColor White
    Write-Host "      3. Username: santoshrohini-001-site44" -ForegroundColor White
    Write-Host "      4. Password: [your FTP password]" -ForegroundColor White
    Write-Host "      5. Upload contents of: $publishFolder" -ForegroundColor White
    Write-Host "      6. Upload to: /wwwroot/" -ForegroundColor White
    Write-Host "      7. Overwrite all existing files" -ForegroundColor White
    
    Write-Host "`n   Opening publish folder..." -ForegroundColor Yellow
    Start-Process "explorer.exe" -ArgumentList $publishFolder
} else {
    Write-Host "   ℹ️  Skipping FTP upload" -ForegroundColor Yellow
    Write-Host "   📁 Published files are ready at: $publishFolder" -ForegroundColor Cyan
}

# Step 9: Post-deployment checklist
Write-Host "`n✅ DEPLOYMENT CHECKLIST:" -ForegroundColor Green
Write-Host ""
Write-Host "   After uploading files to SmarterASP.NET:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   1. ✅ Login to Control Panel:" -ForegroundColor White
Write-Host "      https://member5-4.smarterasp.net/cp/cp_screen" -ForegroundColor Cyan
Write-Host ""
Write-Host "   2. ✅ Set .NET Version:" -ForegroundColor White
Write-Host "      Control Panel → Settings → .NET Version → Select '.NET 8.0'" -ForegroundColor White
Write-Host ""
Write-Host "   3. ✅ Enable ASP.NET Core Module:" -ForegroundColor White
Write-Host "      Control Panel → ASP.NET Core Module → Ensure 'Enabled'" -ForegroundColor White
Write-Host ""
Write-Host "   4. ✅ Recycle Application Pool:" -ForegroundColor White
Write-Host "      Control Panel → Application Pool → Click 'Recycle'" -ForegroundColor White
Write-Host ""
Write-Host "   5. ✅ Test URLs:" -ForegroundColor White
Write-Host "      Root: http://santoshrohini-001-site44.qtempurl.com/" -ForegroundColor Cyan
Write-Host "      Swagger: http://santoshrohini-001-site44.qtempurl.com/swagger" -ForegroundColor Cyan
Write-Host "      API Test: http://santoshrohini-001-site44.qtempurl.com/api/Auth/login" -ForegroundColor Cyan
Write-Host ""
Write-Host "   6. ✅ Check Logs (if errors occur):" -ForegroundColor White
Write-Host "      Control Panel → Log Manager → View Application Logs" -ForegroundColor White
Write-Host "      Or check: /logs/stdout files in your FTP" -ForegroundColor White
Write-Host ""

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "🎉 Deployment preparation complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

