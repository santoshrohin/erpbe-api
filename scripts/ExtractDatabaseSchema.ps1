# Script to extract actual database schema and stored procedures
# This will connect to the actual database and extract all table definitions and stored procedures

param(
    [string]$ConnectionString = "",
    [string]$OutputPath = ".\Database_Scripts"
)

if ([string]::IsNullOrEmpty($ConnectionString)) {
    Write-Host "Error: ConnectionString is required"
    Write-Host "Usage: .\ExtractDatabaseSchema.ps1 -ConnectionString 'Server=...;Database=...;User Id=...;Password=...'"
    exit 1
}

# Create output directories
$tablesPath = Join-Path $OutputPath "Tables"
$sprocsPath = Join-Path $OutputPath "StoredProcedures"
New-Item -ItemType Directory -Force -Path $tablesPath | Out-Null
New-Item -ItemType Directory -Force -Path $sprocsPath | Out-Null

# SQL query to get all table schemas
$getTablesQuery = @"
SELECT 
    t.name AS TableName,
    'CREATE TABLE [dbo].[' + t.name + '] (' + CHAR(13) + CHAR(10) +
    STUFF((
        SELECT 
            ',    [' + c.name + '] ' +
            CASE 
                WHEN ty.name = 'varchar' THEN 'VARCHAR(' + CAST(c.max_length AS VARCHAR) + ')'
                WHEN ty.name = 'nvarchar' THEN 'NVARCHAR(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length/2 AS VARCHAR) END + ')'
                WHEN ty.name = 'char' THEN 'CHAR(' + CAST(c.max_length AS VARCHAR) + ')'
                WHEN ty.name = 'nchar' THEN 'NCHAR(' + CAST(c.max_length/2 AS VARCHAR) + ')'
                WHEN ty.name = 'decimal' THEN 'DECIMAL(' + CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR) + ')'
                WHEN ty.name = 'numeric' THEN 'NUMERIC(' + CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR) + ')'
                WHEN ty.name = 'float' THEN 'FLOAT'
                WHEN ty.name = 'real' THEN 'REAL'
                WHEN ty.name = 'int' THEN 'INT'
                WHEN ty.name = 'bigint' THEN 'BIGINT'
                WHEN ty.name = 'smallint' THEN 'SMALLINT'
                WHEN ty.name = 'tinyint' THEN 'TINYINT'
                WHEN ty.name = 'bit' THEN 'BIT'
                WHEN ty.name = 'datetime' THEN 'DATETIME'
                WHEN ty.name = 'datetime2' THEN 'DATETIME2'
                WHEN ty.name = 'date' THEN 'DATE'
                WHEN ty.name = 'time' THEN 'TIME'
                WHEN ty.name = 'timestamp' THEN 'TIMESTAMP'
                WHEN ty.name = 'uniqueidentifier' THEN 'UNIQUEIDENTIFIER'
                WHEN ty.name = 'xml' THEN 'XML'
                ELSE ty.name
            END +
            CASE WHEN c.is_nullable = 0 THEN ' NOT NULL' ELSE ' NULL' END +
            CASE WHEN c.is_identity = 1 THEN ' IDENTITY(1,1)' ELSE '' END +
            CASE 
                WHEN dc.definition IS NOT NULL THEN ' DEFAULT ' + dc.definition
                ELSE ''
            END
        FROM sys.columns c
        INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
        WHERE c.object_id = t.object_id
        ORDER BY c.column_id
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') +
    CHAR(13) + CHAR(10) + ');' AS CreateTableScript
FROM sys.tables t
WHERE t.is_ms_shipped = 0
ORDER BY t.name;
"@

# SQL query to get all stored procedures
$getSprocsQuery = @"
SELECT 
    s.name AS SchemaName,
    p.name AS ProcedureName,
    OBJECT_DEFINITION(p.object_id) AS ProcedureDefinition
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE p.is_ms_shipped = 0
ORDER BY s.name, p.name;
"@

try {
    # Connect to database using SqlClient
    Add-Type -Path "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\*\System.Data.SqlClient.dll" -ErrorAction SilentlyContinue
    Add-Type -Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Data.dll" -ErrorAction SilentlyContinue
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    Write-Host "Connected to database successfully"
    Write-Host "Extracting table schemas..."
    
    # Get tables
    $tablesCommand = New-Object System.Data.SqlClient.SqlCommand($getTablesQuery, $connection)
    $tablesAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($tablesCommand)
    $tablesDataSet = New-Object System.Data.DataSet
    $tablesAdapter.Fill($tablesDataSet) | Out-Null
    
    foreach ($row in $tablesDataSet.Tables[0].Rows) {
        $tableName = $row["TableName"]
        $createScript = $row["CreateTableScript"]
        
        $filePath = Join-Path $tablesPath "$tableName.sql"
        $createScript | Out-File -FilePath $filePath -Encoding UTF8
        Write-Host "  Extracted: $tableName"
    }
    
    Write-Host "Extracting stored procedures..."
    
    # Get stored procedures
    $sprocsCommand = New-Object System.Data.SqlClient.SqlCommand($getSprocsQuery, $connection)
    $sprocsAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($sprocsCommand)
    $sprocsDataSet = New-Object System.Data.DataSet
    $sprocsAdapter.Fill($sprocsDataSet) | Out-Null
    
    foreach ($row in $sprocsDataSet.Tables[0].Rows) {
        $schemaName = $row["SchemaName"]
        $procName = $row["ProcedureName"]
        $procDefinition = $row["ProcedureDefinition"]
        
        $filePath = Join-Path $sprocsPath "$procName.sql"
        $procDefinition | Out-File -FilePath $filePath -Encoding UTF8
        Write-Host "  Extracted: $procName"
    }
    
    $connection.Close()
    Write-Host "`nExtraction complete!"
    Write-Host "Tables saved to: $tablesPath"
    Write-Host "Stored Procedures saved to: $sprocsPath"
    
} catch {
    Write-Host "Error: $_"
    Write-Host $_.Exception.Message
    if ($connection -ne $null) {
        $connection.Close()
    }
    exit 1
}

