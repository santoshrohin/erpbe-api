# Sync Production Schema to Test Database
param(
    [string]$ProductionConnectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=true;",
    [string]$TestConnectionString = "Server=localhost,1434;Database=ErpBE_Test;User Id=sa;Password=TestPassword123!;TrustServerCertificate=true;"
)

Write-Host "🔄 Syncing Production Schema to Test Database..." -ForegroundColor Green

# Function to execute SQL and return results
function Invoke-SqlQuery {
    param(
        [string]$ConnectionString,
        [string]$Query
    )
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $command = New-Object System.Data.SqlClient.SqlCommand($Query, $connection)
    $connection.Open()
    
    try {
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset)
        return $dataset.Tables[0]
    }
    finally {
        $connection.Close()
    }
}

# Function to execute SQL without returning results
function Invoke-SqlCommand {
    param(
        [string]$ConnectionString,
        [string]$Query
    )
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $command = New-Object System.Data.SqlClient.SqlCommand($Query, $connection)
    $connection.Open()
    
    try {
        $command.ExecuteNonQuery() | Out-Null
    }
    finally {
        $connection.Close()
    }
}

Write-Host "📋 Getting table list from production..." -ForegroundColor Yellow
$tablesQuery = @"
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
AND TABLE_NAME IN ('USER_MASTER', 'ROLES', 'UserRoles', 'ITEM_UNIT_MASTER', 'AUDIT_TRAIL')
ORDER BY TABLE_NAME
"@

$productionTables = Invoke-SqlQuery -ConnectionString $ProductionConnectionString -Query $tablesQuery

Write-Host "🔍 Comparing table schemas..." -ForegroundColor Yellow
foreach ($table in $productionTables) {
    $tableName = $table.TABLE_NAME
    Write-Host "  Checking table: $tableName" -ForegroundColor Cyan
    
    # Get production table schema
    $schemaQuery = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = '$tableName'
ORDER BY ORDINAL_POSITION
"@
    
    $productionSchema = Invoke-SqlQuery -ConnectionString $ProductionConnectionString -Query $schemaQuery
    
    # Get test table schema
    $testSchema = Invoke-SqlQuery -ConnectionString $TestConnectionString -Query $schemaQuery
    
    # Compare schemas and generate ALTER statements
    $missingColumns = @()
    foreach ($prodColumn in $productionSchema) {
        $testColumn = $testSchema | Where-Object { $_.COLUMN_NAME -eq $prodColumn.COLUMN_NAME }
        if (-not $testColumn) {
            $missingColumns += $prodColumn
        }
    }
    
    # Add missing columns to test database
    foreach ($column in $missingColumns) {
        $dataType = $column.DATA_TYPE
        if ($column.CHARACTER_MAXIMUM_LENGTH) {
            $dataType += "($($column.CHARACTER_MAXIMUM_LENGTH))"
        }
        
        $nullConstraint = if ($column.IS_NULLABLE -eq "NO") { "NOT NULL" } else { "NULL" }
        $defaultValue = if ($column.COLUMN_DEFAULT) { "DEFAULT $($column.COLUMN_DEFAULT)" } else { "" }
        
        $alterQuery = "ALTER TABLE [$tableName] ADD [$($column.COLUMN_NAME)] $dataType $nullConstraint $defaultValue"
        
        Write-Host "    Adding column: $($column.COLUMN_NAME)" -ForegroundColor Green
        Invoke-SqlCommand -ConnectionString $TestConnectionString -Query $alterQuery
    }
}

Write-Host "📋 Getting stored procedures from production..." -ForegroundColor Yellow
$proceduresQuery = @"
SELECT 
    ROUTINE_NAME,
    ROUTINE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE' 
AND ROUTINE_NAME LIKE 'SP_%'
ORDER BY ROUTINE_NAME
"@

$productionProcedures = Invoke-SqlQuery -ConnectionString $ProductionConnectionString -Query $proceduresQuery

Write-Host "🔄 Updating stored procedures..." -ForegroundColor Yellow
foreach ($proc in $productionProcedures) {
    $procName = $proc.ROUTINE_NAME
    $procDefinition = $proc.ROUTINE_DEFINITION
    
    Write-Host "  Updating procedure: $procName" -ForegroundColor Cyan
    
    # Drop and recreate procedure
    $dropQuery = "IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = '$procName') DROP PROCEDURE [dbo].[$procName]"
    Invoke-SqlCommand -ConnectionString $TestConnectionString -Query $dropQuery
    
    # Create procedure
    Invoke-SqlCommand -ConnectionString $TestConnectionString -Query $procDefinition
}

Write-Host "✅ Schema sync completed!" -ForegroundColor Green
Write-Host "🔄 You may need to update test data if new columns were added." -ForegroundColor Yellow
