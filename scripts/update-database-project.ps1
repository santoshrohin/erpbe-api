# Update Database Project from Production
param(
    [string]$ProductionConnectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=true;",
    [string]$DatabaseProjectPath = "ErpBE.Database"
)

Write-Host "🔄 Updating Database Project from Production..." -ForegroundColor Green

# Function to get table schema as CREATE TABLE statement
function Get-TableSchema {
    param(
        [string]$ConnectionString,
        [string]$TableName
    )
    
    $query = @"
SELECT 
    'CREATE TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '] (' + CHAR(13) + CHAR(10) +
    STRING_AGG(
        '    [' + COLUMN_NAME + '] ' + 
        DATA_TYPE + 
        CASE 
            WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')'
            WHEN NUMERIC_PRECISION IS NOT NULL AND NUMERIC_SCALE IS NOT NULL THEN '(' + CAST(NUMERIC_PRECISION AS VARCHAR) + ',' + CAST(NUMERIC_SCALE AS VARCHAR) + ')'
            ELSE ''
        END +
        CASE WHEN IS_NULLABLE = 'NO' THEN ' NOT NULL' ELSE ' NULL' END +
        CASE WHEN COLUMN_DEFAULT IS NOT NULL THEN ' DEFAULT ' + COLUMN_DEFAULT ELSE '' END,
        ',' + CHAR(13) + CHAR(10)
    ) + CHAR(13) + CHAR(10) + ');' AS CREATE_STATEMENT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = '$TableName'
GROUP BY TABLE_SCHEMA, TABLE_NAME
"@
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
    $connection.Open()
    
    try {
        $result = $command.ExecuteScalar()
        return $result
    }
    finally {
        $connection.Close()
    }
}

# Function to get stored procedure definition
function Get-StoredProcedureDefinition {
    param(
        [string]$ConnectionString,
        [string]$ProcedureName
    )
    
    $query = @"
SELECT 
    'CREATE PROCEDURE [dbo].[' + ROUTINE_NAME + ']' + CHAR(13) + CHAR(10) +
    ROUTINE_DEFINITION + CHAR(13) + CHAR(10) + 'GO' AS PROCEDURE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_NAME = '$ProcedureName'
"@
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
    $connection.Open()
    
    try {
        $result = $command.ExecuteScalar()
        return $result
    }
    finally {
        $connection.Close()
    }
}

# Get list of tables to update
$tablesQuery = @"
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
AND TABLE_NAME IN ('USER_MASTER', 'ROLES', 'UserRoles', 'ITEM_UNIT_MASTER', 'AUDIT_TRAIL')
ORDER BY TABLE_NAME
"@

$connection = New-Object System.Data.SqlClient.SqlConnection($ProductionConnectionString)
$command = New-Object System.Data.SqlClient.SqlCommand($tablesQuery, $connection)
$connection.Open()

try {
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset)
    $tables = $dataset.Tables[0]
}
finally {
    $connection.Close()
}

# Update table definitions
foreach ($table in $tables) {
    $tableName = $table.TABLE_NAME
    Write-Host "📋 Updating table: $tableName" -ForegroundColor Cyan
    
    $schema = Get-TableSchema -ConnectionString $ProductionConnectionString -TableName $tableName
    $filePath = Join-Path $DatabaseProjectPath "Tables\$tableName.sql"
    
    Set-Content -Path $filePath -Value $schema -Encoding UTF8
}

# Get list of stored procedures to update
$proceduresQuery = @"
SELECT ROUTINE_NAME 
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE' 
AND ROUTINE_NAME LIKE 'SP_%'
ORDER BY ROUTINE_NAME
"@

$connection = New-Object System.Data.SqlClient.SqlConnection($ProductionConnectionString)
$command = New-Object System.Data.SqlClient.SqlCommand($proceduresQuery, $connection)
$connection.Open()

try {
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset)
    $procedures = $dataset.Tables[0]
}
finally {
    $connection.Close()
}

# Update stored procedure definitions
foreach ($proc in $procedures) {
    $procName = $proc.ROUTINE_NAME
    Write-Host "📋 Updating procedure: $procName" -ForegroundColor Cyan
    
    $definition = Get-StoredProcedureDefinition -ConnectionString $ProductionConnectionString -TableName $procName
    $filePath = Join-Path $DatabaseProjectPath "StoredProcedures\$procName.sql"
    
    Set-Content -Path $filePath -Value $definition -Encoding UTF8
}

Write-Host "✅ Database project updated!" -ForegroundColor Green
Write-Host "🔄 Next steps:" -ForegroundColor Yellow
Write-Host "  1. Review the changes in the database project" -ForegroundColor White
Write-Host "  2. Update test data if new columns were added" -ForegroundColor White
Write-Host "  3. Run: .\scripts\reset-test-db.ps1" -ForegroundColor White
Write-Host "  4. Run tests to ensure everything works" -ForegroundColor White
