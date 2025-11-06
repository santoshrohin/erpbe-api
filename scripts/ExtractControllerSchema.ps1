# Script to extract database schema for a specific controller
# Usage: .\ExtractControllerSchema.ps1 -ControllerName "Company"

param(
    [Parameter(Mandatory=$true)]
    [string]$ControllerName,
    
    [string]$ConnectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;"
)

$basePath = Join-Path $PSScriptRoot ".." ".."
$tablesPath = Join-Path $basePath "Database_Scripts" "Tables"
$sprocsPath = Join-Path $basePath "Database_Scripts" "StoredProcedures"
$controllerPath = Join-Path $basePath "Database_Scripts" "Controllers" $ControllerName

New-Item -ItemType Directory -Force -Path $tablesPath | Out-Null
New-Item -ItemType Directory -Force -Path $sprocsPath | Out-Null
New-Item -ItemType Directory -Force -Path $controllerPath | Out-Null

Write-Host "Extracting schema for controller: $ControllerName"
Write-Host "Connection: $ConnectionString"
Write-Host "Output: $controllerPath"

# This will be implemented to extract tables and stored procedures
# For now, we'll use the C# extractor for each controller

