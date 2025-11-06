using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtractDatabaseSchema
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;";
            
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
            string tablesPath = Path.GetFullPath(Path.Combine(basePath, "Database_Scripts", "Tables"));
            string sprocsPath = Path.GetFullPath(Path.Combine(basePath, "Database_Scripts", "StoredProcedures"));
            
            Directory.CreateDirectory(tablesPath);
            Directory.CreateDirectory(sprocsPath);
            
            Console.WriteLine($"Extracting to:");
            Console.WriteLine($"  Tables: {tablesPath}");
            Console.WriteLine($"  Stored Procedures: {sprocsPath}");
            Console.WriteLine("\nConnecting to database...");
            
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
            
            var tables = new List<(string Schema, string Name)>();
            
            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string schemaName = reader["SchemaName"].ToString() ?? "dbo";
                    string tableName = reader["TableName"].ToString() ?? "";
                    
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tables.Add((schemaName, tableName));
                    }
                }
            }
            
            // Now extract each table (after closing the reader)
            foreach (var (schemaName, tableName) in tables)
            {
                Console.WriteLine($"  Extracting: {schemaName}.{tableName}");
                
                // Generate CREATE TABLE script
                string createScript = GenerateCreateTableScript(connection, schemaName, tableName);
                
                string filePath = Path.Combine(outputPath, $"{tableName}.sql");
                File.WriteAllText(filePath, createScript, Encoding.UTF8);
            }
        }
        
        static string GenerateCreateTableScript(SqlConnection connection, string schemaName, string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"-- Table: {schemaName}.{tableName}");
            sb.AppendLine($"-- Generated from actual database schema");
            sb.AppendLine($"-- Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{schemaName}].[{tableName}]') AND type in (N'U'))");
            sb.AppendLine($"BEGIN");
            sb.AppendLine($"    CREATE TABLE [{schemaName}].[{tableName}] (");
            
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
                    CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
                FROM sys.columns c
                INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
                LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
                LEFT JOIN (
                    SELECT 
                        ic.column_id
                    FROM sys.index_columns ic
                    INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                    WHERE i.is_primary_key = 1
                        AND ic.object_id = OBJECT_ID(@TableName)
                ) pk ON c.column_id = pk.column_id
                WHERE c.object_id = OBJECT_ID(@TableName)
                ORDER BY c.column_id;
            ";
            
            var columns = new List<string>();
            var columnData = new List<(string Name, string Type, int MaxLength, byte Precision, byte Scale, bool IsNullable, bool IsIdentity, string? DefaultDef)>();
            
            using (var command = new SqlCommand(columnsQuery, connection))
            {
                command.Parameters.AddWithValue("@TableName", $"{schemaName}.{tableName}");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string colName = reader["ColumnName"].ToString() ?? "";
                        string typeName = reader["TypeName"].ToString() ?? "";
                        int maxLength = reader["MaxLength"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MaxLength"]);
                        byte precision = reader["Precision"] == DBNull.Value ? (byte)0 : Convert.ToByte(reader["Precision"]);
                        byte scale = reader["Scale"] == DBNull.Value ? (byte)0 : Convert.ToByte(reader["Scale"]);
                        bool isNullable = Convert.ToBoolean(reader["IsNullable"]);
                        bool isIdentity = Convert.ToBoolean(reader["IsIdentity"]);
                        string? defaultDef = reader["DefaultDefinition"]?.ToString();
                        
                        columnData.Add((colName, typeName, maxLength, precision, scale, isNullable, isIdentity, defaultDef));
                    }
                }
            }
            
            // Now build column definitions
            foreach (var (colName, typeName, maxLength, precision, scale, isNullable, isIdentity, defaultDef) in columnData)
            {
                var colDef = new StringBuilder();
                colDef.Append($"        [{colName}] ");
                
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
                else if (typeName == "float")
                {
                    colDef.Append("FLOAT");
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
                    string cleanedDefaultDef = defaultDef.Trim();
                    if (cleanedDefaultDef.StartsWith("(") && cleanedDefaultDef.EndsWith(")"))
                        cleanedDefaultDef = cleanedDefaultDef.Substring(1, cleanedDefaultDef.Length - 2);
                    
                    if (cleanedDefaultDef.StartsWith("'") && cleanedDefaultDef.EndsWith("'"))
                        colDef.Append($" DEFAULT {cleanedDefaultDef}");
                    else if (cleanedDefaultDef.Contains("GETDATE") || cleanedDefaultDef.Contains("GETUTCDATE"))
                        colDef.Append($" DEFAULT {cleanedDefaultDef}");
                    else
                        colDef.Append($" DEFAULT {cleanedDefaultDef}");
                }
                
                columns.Add(colDef.ToString());
            }
            
            sb.AppendLine(string.Join("," + Environment.NewLine, columns));
            sb.AppendLine($"    );");
            sb.AppendLine($"END");
            
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
                    string schemaName = reader["SchemaName"].ToString() ?? "dbo";
                    string procName = reader["ProcedureName"].ToString() ?? "";
                    string? procDef = reader["ProcedureDefinition"]?.ToString();
                    
                    if (string.IsNullOrEmpty(procDef) || string.IsNullOrEmpty(procName))
                        continue;
                    
                    Console.WriteLine($"  Extracting: {schemaName}.{procName}");
                    
                    // Add header
                    var sb = new StringBuilder();
                    sb.AppendLine($"-- Stored Procedure: {schemaName}.{procName}");
                    sb.AppendLine($"-- Generated from actual database");
                    sb.AppendLine($"-- Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine();
                    
                    // Remove USE statements
                    procDef = Regex.Replace(
                        procDef,
                        @"^\s*USE\s+\[?[^\]]+\]?\s*;?\s*$",
                        "",
                        RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    
                    // Remove PRINT statements with GO
                    procDef = Regex.Replace(
                        procDef,
                        @"^\s*PRINT\s+.*?GO\s*$",
                        "",
                        RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    
                    sb.Append(procDef);
                    
                    string filePath = Path.Combine(outputPath, $"{procName}.sql");
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
        }
    }
}
