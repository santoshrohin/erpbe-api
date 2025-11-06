using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace ExtractDatabaseSchema
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;";
            
            string tablesPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Database_Scripts", "Tables");
            string sprocsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Database_Scripts", "StoredProcedures");
            
            Directory.CreateDirectory(tablesPath);
            Directory.CreateDirectory(sprocsPath);
            
            Console.WriteLine("Connecting to database...");
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected successfully!");
                
                // Extract all table schemas
                Console.WriteLine("\nExtracting table schemas...");
                ExtractTables(connection, tablesPath);
                
                // Extract all stored procedures
                Console.WriteLine("\nExtracting stored procedures...");
                ExtractStoredProcedures(connection, sprocsPath);
                
                Console.WriteLine("\nExtraction complete!");
            }
        }
        
        static void ExtractTables(SqlConnection connection, string outputPath)
        {
            string query = @"
                SELECT 
                    t.name AS TableName,
                    SCHEMA_NAME(t.schema_id) AS SchemaName
                FROM sys.tables t
                WHERE t.is_ms_shipped = 0
                ORDER BY t.name;
            ";
            
            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string schemaName = reader["SchemaName"].ToString();
                    string tableName = reader["TableName"].ToString();
                    
                    Console.WriteLine($"  Extracting: {schemaName}.{tableName}");
                    
                    // Generate CREATE TABLE script
                    string createScript = GenerateCreateTableScript(connection, schemaName, tableName);
                    
                    string filePath = Path.Combine(outputPath, $"{tableName}.sql");
                    File.WriteAllText(filePath, createScript, Encoding.UTF8);
                }
            }
        }
        
        static string GenerateCreateTableScript(SqlConnection connection, string schemaName, string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"-- Table: {schemaName}.{tableName}");
            sb.AppendLine($"-- Generated from actual database schema");
            sb.AppendLine($"-- Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine($"CREATE TABLE [{schemaName}].[{tableName}] (");
            
            // Get columns
            string columnsQuery = @"
                SELECT 
                    c.name AS ColumnName,
                    ty.name AS TypeName,
                    c.max_length AS MaxLength,
                    c.precision AS Precision,
                    c.scale AS Scale,
                    c.is_nullable AS IsNullable,
                    c.is_identity AS IsIdentity,
                    dc.definition AS DefaultDefinition,
                    pk.is_primary_key AS IsPrimaryKey
                FROM sys.columns c
                INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
                LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
                LEFT JOIN (
                    SELECT 
                        ic.column_id,
                        1 AS is_primary_key
                    FROM sys.index_columns ic
                    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                    WHERE i.is_primary_key = 1
                        AND ic.object_id = OBJECT_ID(@TableName)
                ) pk ON c.column_id = pk.column_id
                WHERE c.object_id = OBJECT_ID(@TableName)
                ORDER BY c.column_id;
            ";
            
            var columns = new List<string>();
            using (var command = new SqlCommand(columnsQuery, connection))
            {
                command.Parameters.AddWithValue("@TableName", $"{schemaName}.{tableName}");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string colName = reader["ColumnName"].ToString();
                        string typeName = reader["TypeName"].ToString();
                        int maxLength = reader["MaxLength"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MaxLength"]);
                        byte precision = reader["Precision"] == DBNull.Value ? (byte)0 : Convert.ToByte(reader["Precision"]);
                        byte scale = reader["Scale"] == DBNull.Value ? (byte)0 : Convert.ToByte(reader["Scale"]);
                        bool isNullable = Convert.ToBoolean(reader["IsNullable"]);
                        bool isIdentity = Convert.ToBoolean(reader["IsIdentity"]);
                        string defaultDef = reader["DefaultDefinition"]?.ToString();
                        bool isPrimaryKey = reader["IsPrimaryKey"] != DBNull.Value && Convert.ToBoolean(reader["IsPrimaryKey"]);
                        
                        var colDef = new StringBuilder();
                        colDef.Append($"    [{colName}] ");
                        
                        // Build type
                        if (typeName == "varchar" || typeName == "char")
                        {
                            colDef.Append($"{typeName.ToUpper()}({maxLength})");
                        }
                        else if (typeName == "nvarchar" || typeName == "nchar")
                        {
                            if (maxLength == -1)
                                colDef.Append($"{typeName.ToUpper()}(MAX)");
                            else
                                colDef.Append($"{typeName.ToUpper()}({maxLength / 2})");
                        }
                        else if (typeName == "decimal" || typeName == "numeric")
                        {
                            colDef.Append($"{typeName.ToUpper()}({precision},{scale})");
                        }
                        else
                        {
                            colDef.Append(typeName.ToUpper());
                        }
                        
                        if (!isNullable)
                            colDef.Append(" NOT NULL");
                        else
                            colDef.Append(" NULL");
                        
                        if (isIdentity)
                            colDef.Append(" IDENTITY(1,1)");
                        
                        if (!string.IsNullOrEmpty(defaultDef))
                        {
                            // Clean up default definition
                            defaultDef = defaultDef.Replace("(", "").Replace(")", "");
                            if (defaultDef.StartsWith("'") && defaultDef.EndsWith("'"))
                                colDef.Append($" DEFAULT {defaultDef}");
                            else if (defaultDef.Contains("GETDATE") || defaultDef.Contains("GETUTCDATE"))
                                colDef.Append($" DEFAULT {defaultDef}");
                            else
                                colDef.Append($" DEFAULT {defaultDef}");
                        }
                        
                        columns.Add(colDef.ToString());
                    }
                }
            }
            
            sb.AppendLine(string.Join("," + Environment.NewLine, columns));
            sb.AppendLine(");");
            
            // Get primary key constraint
            string pkQuery = @"
                SELECT 
                    i.name AS IndexName,
                    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS ColumnNames
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE i.is_primary_key = 1
                    AND i.object_id = OBJECT_ID(@TableName)
                GROUP BY i.name;
            ";
            
            using (var command = new SqlCommand(pkQuery, connection))
            {
                command.Parameters.AddWithValue("@TableName", $"{schemaName}.{tableName}");
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string indexName = reader["IndexName"].ToString();
                        string columnNames = reader["ColumnNames"].ToString();
                        sb.AppendLine();
                        sb.AppendLine($"ALTER TABLE [{schemaName}].[{tableName}]");
                        sb.AppendLine($"ADD CONSTRAINT [{indexName}] PRIMARY KEY ({columnNames});");
                    }
                }
            }
            
            return sb.ToString();
        }
        
        static void ExtractStoredProcedures(SqlConnection connection, string outputPath)
        {
            string query = @"
                SELECT 
                    s.name AS SchemaName,
                    p.name AS ProcedureName,
                    OBJECT_DEFINITION(p.object_id) AS ProcedureDefinition
                FROM sys.procedures p
                INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
                WHERE p.is_ms_shipped = 0
                ORDER BY s.name, p.name;
            ";
            
            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string schemaName = reader["SchemaName"].ToString();
                    string procName = reader["ProcedureName"].ToString();
                    string procDef = reader["ProcedureDefinition"]?.ToString();
                    
                    if (string.IsNullOrEmpty(procDef))
                        continue;
                    
                    Console.WriteLine($"  Extracting: {schemaName}.{procName}");
                    
                    // Add header
                    var sb = new StringBuilder();
                    sb.AppendLine($"-- Stored Procedure: {schemaName}.{procName}");
                    sb.AppendLine($"-- Generated from actual database");
                    sb.AppendLine($"-- Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine();
                    
                    // Remove USE statements
                    procDef = System.Text.RegularExpressions.Regex.Replace(
                        procDef,
                        @"^\s*USE\s+\[?[^\]]+\]?\s*;?\s*$",
                        "",
                        System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    sb.Append(procDef);
                    
                    string filePath = Path.Combine(outputPath, $"{procName}.sql");
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
        }
    }
}

