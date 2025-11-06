# Script to extract complete database schema using SqlServer module
# Shows progress throughout

param(
    [string]$ServerName = "SQL5111.site4now.net",
    [string]$DatabaseName = "db_a2ea4b_sunv2",
    [string]$Username = "db_a2ea4b_sunv2_admin",
    [string]$Password = "abcd@1234",
    [string]$OutputPath = "..\..\Database_Scripts\FullSchema"
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Database Schema Extraction Tool" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Import SqlServer module
Write-Host "[1/5] Importing SqlServer module..." -ForegroundColor Yellow
try {
    Import-Module SqlServer -ErrorAction Stop
    Write-Host "      ✓ SqlServer module loaded" -ForegroundColor Green
} catch {
    Write-Host "      ✗ Failed to load SqlServer module: $_" -ForegroundColor Red
    Write-Host "      Installing SqlServer module..." -ForegroundColor Yellow
    Install-Module -Name SqlServer -Scope CurrentUser -Force -AllowClobber -SkipPublisherCheck
    Import-Module SqlServer
    Write-Host "      ✓ SqlServer module installed and loaded" -ForegroundColor Green
}
Write-Host ""

# Build connection string
$connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
Write-Host "[2/5] Connecting to database..." -ForegroundColor Yellow
Write-Host "      Server: $ServerName" -ForegroundColor Gray
Write-Host "      Database: $DatabaseName" -ForegroundColor Gray

try {
    $connection = New-Object Microsoft.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "      ✓ Connected successfully" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Host "      ✗ Connection failed: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Create output directory
$fullOutputPath = Resolve-Path $OutputPath -ErrorAction SilentlyContinue
if (-not $fullOutputPath) {
    $fullOutputPath = Join-Path $PSScriptRoot $OutputPath
    New-Item -ItemType Directory -Force -Path $fullOutputPath | Out-Null
}
Write-Host "[3/5] Output directory: $fullOutputPath" -ForegroundColor Yellow
Write-Host ""

# Extract tables
Write-Host "[4/5] Extracting tables..." -ForegroundColor Yellow
$tablesPath = Join-Path $fullOutputPath "Tables"
New-Item -ItemType Directory -Force -Path $tablesPath | Out-Null

$tablesQuery = @"
SELECT 
    t.name AS TableName,
    SCHEMA_NAME(t.schema_id) AS SchemaName
FROM sys.tables t
WHERE t.is_ms_shipped = 0
ORDER BY t.name;
"@

$tables = Invoke-Sqlcmd -ConnectionString $connectionString -Query $tablesQuery
$tableCount = $tables.Count
Write-Host "      Found $tableCount tables" -ForegroundColor Gray

$tableIndex = 0
foreach ($table in $tables) {
    $tableIndex++
    $schemaName = $table.SchemaName
    $tableName = $table.TableName
    $percent = [math]::Round(($tableIndex / $tableCount) * 100, 1)
    
    Write-Progress -Activity "Extracting Tables" -Status "Processing $tableName" -PercentComplete $percent
    
    # Generate CREATE TABLE script using SMO
    $dateStr = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $script = @"
-- Table: $schemaName.$tableName
-- Generated from actual database schema
-- Date: $dateStr

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[$schemaName].[$tableName]') AND type in (N'U'))
BEGIN
    CREATE TABLE [$schemaName].[$tableName] (
"@
    
    # Get columns
    $columnsQuery = @"
SELECT 
    c.name AS ColumnName,
    ty.name AS TypeName,
    c.max_length AS MaxLength,
    c.precision AS Precision,
    c.scale AS Scale,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    dc.definition AS DefaultDefinition
FROM sys.columns c
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID('$schemaName.$tableName')
ORDER BY c.column_id;
"@
    
    $columns = Invoke-Sqlcmd -ConnectionString $connectionString -Query $columnsQuery
    $columnDefs = @()
    
    foreach ($col in $columns) {
        $colName = $col.ColumnName
        $typeName = $col.TypeName
        $maxLength = $col.MaxLength
        $precision = $col.Precision
        $scale = $col.Scale
        $isNullable = $col.IsNullable
        $isIdentity = $col.IsIdentity
        $defaultDef = $col.DefaultDefinition
        
        $typeDef = switch ($typeName) {
            "varchar" { if ($maxLength -eq -1) { "VARCHAR(MAX)" } else { "VARCHAR($maxLength)" } }
            "nvarchar" { if ($maxLength -eq -1) { "NVARCHAR(MAX)" } else { "NVARCHAR($($maxLength/2))" } }
            "char" { "CHAR($maxLength)" }
            "nchar" { "NCHAR($($maxLength/2))" }
            "decimal" { "DECIMAL($precision,$scale)" }
            "numeric" { "NUMERIC($precision,$scale)" }
            default { $typeName.ToUpper() }
        }
        
        $nullability = if ($isNullable) { "NULL" } else { "NOT NULL" }
        $identity = if ($isIdentity) { " IDENTITY(1,1)" } else { "" }
        $default = if ($defaultDef) { " DEFAULT $defaultDef" } else { "" }
        
        $columnDefs += "        [$colName] $typeDef $nullability$identity$default"
    }
    
    $script += "`n" + ($columnDefs -join ",`n")
    $script += "`n    );`nEND"
    
    # Get primary key
    $pkQuery = @"
SELECT 
    i.name AS IndexName,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS ColumnNames
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.is_primary_key = 1
    AND i.object_id = OBJECT_ID('$schemaName.$tableName')
GROUP BY i.name;
"@
    
    try {
        $pk = Invoke-Sqlcmd -ConnectionString $connectionString -Query $pkQuery -ErrorAction SilentlyContinue
        if ($pk) {
            $script += "`n`nALTER TABLE [$schemaName].[$tableName]`n"
            $script += "ADD CONSTRAINT [$($pk.IndexName)] PRIMARY KEY ($($pk.ColumnNames));"
        }
    } catch {
        # No primary key or error getting it
    }
    
    $filePath = Join-Path $tablesPath "$tableName.sql"
    Set-Content -Path $filePath -Value $script -Encoding UTF8
    
    Write-Host "      [$tableIndex/$tableCount] $tableName" -ForegroundColor Gray
}

Write-Progress -Activity "Extracting Tables" -Completed
Write-Host "      ✓ Extracted $tableCount tables" -ForegroundColor Green
Write-Host ""

# Extract stored procedures
Write-Host "[5/5] Extracting stored procedures..." -ForegroundColor Yellow
$sprocsPath = Join-Path $fullOutputPath "StoredProcedures"
New-Item -ItemType Directory -Force -Path $sprocsPath | Out-Null

$sprocsQuery = @"
SELECT 
    s.name AS SchemaName,
    p.name AS ProcedureName,
    OBJECT_DEFINITION(p.object_id) AS ProcedureDefinition
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE p.is_ms_shipped = 0
ORDER BY s.name, p.name;
"@

$sprocs = Invoke-Sqlcmd -ConnectionString $connectionString -Query $sprocsQuery
$sprocCount = $sprocs.Count
Write-Host "      Found $sprocCount stored procedures" -ForegroundColor Gray

$sprocIndex = 0
foreach ($sproc in $sprocs) {
    $sprocIndex++
    $schemaName = $sproc.SchemaName
    $procName = $sproc.ProcedureName
    $procDef = $sproc.ProcedureDefinition
    $percent = [math]::Round(($sprocIndex / $sprocCount) * 100, 1)
    
    Write-Progress -Activity "Extracting Stored Procedures" -Status "Processing $procName" -PercentComplete $percent
    
    if ($procDef) {
        $dateStr = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
        $script = @"
-- Stored Procedure: $schemaName.$procName
-- Generated from actual database
-- Date: $dateStr

$procDef
"@
        
        # Remove USE statements
        $script = $script -replace '(?m)^\s*USE\s+\[?[^\]]+\]?\s*;?\s*$', ''
        
        $filePath = Join-Path $sprocsPath "$procName.sql"
        Set-Content -Path $filePath -Value $script -Encoding UTF8
        
        Write-Host "      [$sprocIndex/$sprocCount] $procName" -ForegroundColor Gray
    }
}

Write-Progress -Activity "Extracting Stored Procedures" -Completed
Write-Host "      ✓ Extracted $sprocCount stored procedures" -ForegroundColor Green
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Extraction Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Tables: $tablesPath" -ForegroundColor White
Write-Host "Stored Procedures: $sprocsPath" -ForegroundColor White
Write-Host ""

