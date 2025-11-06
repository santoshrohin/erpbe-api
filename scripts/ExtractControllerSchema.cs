using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtractControllerSchema
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ExtractControllerSchema <ControllerName>");
                Console.WriteLine("Example: ExtractControllerSchema Company");
                return;
            }

            string controllerName = args[0];
            string connectionString = "Server=SQL5111.site4now.net;Database=db_a2ea4b_sunv2;User Id=db_a2ea4b_sunv2_admin;Password=abcd@1234;TrustServerCertificate=True;";
            
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
            string tablesPath = Path.GetFullPath(Path.Combine(basePath, "Database_Scripts", "Tables"));
            string sprocsPath = Path.GetFullPath(Path.Combine(basePath, "Database_Scripts", "StoredProcedures"));
            string controllerPath = Path.GetFullPath(Path.Combine(basePath, "Database_Scripts", "Controllers", controllerName));
            
            Directory.CreateDirectory(tablesPath);
            Directory.CreateDirectory(sprocsPath);
            Directory.CreateDirectory(controllerPath);
            
            Console.WriteLine($"Extracting schema for controller: {controllerName}");
            Console.WriteLine($"Tables: {tablesPath}");
            Console.WriteLine($"Stored Procedures: {sprocsPath}");
            Console.WriteLine($"Controller specific: {controllerPath}");
            
            // Map controller to tables and stored procedures
            var controllerMapping = GetControllerMapping();
            
            if (!controllerMapping.ContainsKey(controllerName))
            {
                Console.WriteLine($"Controller {controllerName} not found in mapping. Available: {string.Join(", ", controllerMapping.Keys)}");
                return;
            }
            
            var mapping = controllerMapping[controllerName];
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected successfully!");
                
                // Extract tables
                Console.WriteLine($"\nExtracting tables for {controllerName}...");
                foreach (var tableName in mapping.Tables)
                {
                    Console.WriteLine($"  Extracting table: {tableName}");
                    string createScript = GenerateCreateTableScript(connection, "dbo", tableName);
                    string filePath = Path.Combine(tablesPath, $"{tableName}.sql");
                    File.WriteAllText(filePath, createScript, Encoding.UTF8);
                }
                
                // Extract stored procedures
                Console.WriteLine($"\nExtracting stored procedures for {controllerName}...");
                foreach (var sprocName in mapping.StoredProcedures)
                {
                    Console.WriteLine($"  Extracting stored procedure: {sprocName}");
                    ExtractStoredProcedure(connection, "dbo", sprocName, sprocsPath, controllerPath);
                }
                
                Console.WriteLine("\nExtraction complete!");
            }
        }
        
        static Dictionary<string, (List<string> Tables, List<string> StoredProcedures)> GetControllerMapping()
        {
            return new Dictionary<string, (List<string>, List<string>)>
            {
                ["Company"] = (new List<string> { "COMPANY_MASTER" }, new List<string> { "ERP_GetActiveCompanies" }),
                ["FinancialYear"] = (new List<string> { "COMPANY_MASTER", "FINANCIAL_YEAR_MASTER" }, new List<string> { "ERP_GetFinancialYearsByCompanyId" }),
                ["Login"] = (new List<string> { "USER_MASTER", "COMPANY_MASTER", "ROLES", "UserRoles" }, new List<string> { "SP_VerifyLogin", "SP_GetUserRoles", "SP_GetUserRolesByUserId" }),
                ["CustomerPo"] = (new List<string> { "CUSTPO_MASTER", "CUSTPO_DETAIL", "PARTY_MASTER", "ITEM_MASTER", "ITEM_UNIT_MASTER", "COMPANY_MASTER" }, 
                    new List<string> { "ERP_GetAllCustomerPos", "ERP_GetCustomerPoById", "ERP_CreateCustomerPo", "ERP_CreateCustomerPoDetail", "ERP_UpdateCustomerPo", "ERP_DeleteCustomerPo", "ERP_GetCustomerPoPrintData", "ERP_CheckCustomerPoLock", "ERP_LockCustomerPo", "ERP_UnlockCustomerPo" }),
                ["UnitMaster"] = (new List<string> { "UNIT_MASTER", "ITEM_UNIT_MASTER", "COMPANY_MASTER" }, 
                    new List<string> { "SP_GetUnitMasters", "SP_GetUnitMasterById", "SP_GetUnitMasterByName", "SP_IsUnitNameUnique", "SP_CreateUnitMaster", "SP_UpdateUnitMaster", "SP_SetUnitMasterActiveStatus" }),
                ["ItemCategoryMaster"] = (new List<string> { "ITEM_CATEGORY_MASTER", "COMPANY_MASTER", "ITEM_MASTER" }, 
                    new List<string> { "SP_GetItemCategoryMasters", "SP_GetItemCategoryMasterById", "SP_GetItemCategoryMasterByName", "SP_IsItemCategoryNameUnique", "SP_CreateItemCategoryMaster", "SP_UpdateItemCategoryMaster", "SP_DeleteItemCategoryMaster", "SP_CheckItemCategoryUsage", "SP_SetItemCategoryActiveStatus" }),
                ["CustomerTypeMaster"] = (new List<string> { "CUSTOMER_TYPE_MASTER", "COMPANY_MASTER", "PARTY_MASTER" }, 
                    new List<string> { "ERP_GetCustomerTypeMasters", "ERP_GetCustomerTypeMasterById", "ERP_GetCustomerTypeMasterByTypeCode", "ERP_CreateCustomerTypeMaster", "ERP_UpdateCustomerTypeMaster", "ERP_DeleteCustomerTypeMaster", "ERP_CheckCustomerTypeUsage", "ERP_IsCustomerTypeModified", "ERP_IsTypeCodeUnique" }),
                ["SoTypeMaster"] = (new List<string> { "SO_TYPE_MASTER", "COMPANY_MASTER" }, 
                    new List<string> { "ERP_GetSoTypeMasters", "ERP_GetSoTypeMasterById", "ERP_GetSoTypeMasterByShortName", "ERP_CreateSoTypeMaster", "ERP_UpdateSoTypeMaster", "ERP_DeleteSoTypeMaster", "ERP_IsSoTypeShortNameUnique", "ERP_CheckSoTypeUsage", "ERP_IsSoTypeFixedRecord" }),
                ["User"] = (new List<string> { "USER_MASTER", "COMPANY_MASTER", "ROLES", "UserRoles" }, 
                    new List<string> { "SP_GetAllUsers", "SP_GetUserById", "SP_GetUserByUsername", "SP_GetUsersByCompany", "SP_CreateUser", "SP_UpdateUser", "SP_UserExists" }),
                ["Role"] = (new List<string> { "ROLES", "UserRoles", "USER_MASTER" }, 
                    new List<string> { "SP_GetAllRoles", "SP_GetActiveRoles", "SP_GetRoleById", "SP_GetRoleByName", "SP_CreateRole", "SP_UpdateRole", "SP_RoleExists" }),
                ["Audit"] = (new List<string> { "AUDIT_TRAIL", "AUDIT_CONFIGURATION" }, 
                    new List<string> { "SP_GetAuditTrail", "SP_GetAuditHistory", "SP_GetAuditEntriesWithFilters", "SP_GetAuditEntryById", "SP_CreateAuditEntry", "SP_GetAuditConfiguration", "SP_GetAllAuditConfigurations", "SP_CreateAuditConfiguration", "SP_UpdateAuditConfiguration" }),
                ["Logs"] = (new List<string> { "Logs" }, 
                    new List<string> { "SP_GetLogs", "SP_GetLogStatistics", "SP_CleanupOldLogs" }),
                ["CustomerMaster"] = (new List<string> { "PARTY_MASTER", "COMPANY_MASTER", "AREA_MASTER", "CUSTOMER_TYPE_MASTER" }, 
                    new List<string> { "ERP_GetCustomerMasters", "ERP_GetCustomerMasterById", "ERP_CreateCustomerMaster", "ERP_UpdateCustomerMaster", "ERP_DeleteCustomerMaster", "ERP_CheckPartyNameUnique", "ERP_CheckAbbreviationUnique" }),
                ["TaxInvoice"] = (new List<string> { "TAX_INVOICE_MASTER", "TAX_INVOICE_DETAIL", "INVOICE_MASTER", "PARTY_MASTER", "ITEM_MASTER", "COMPANY_MASTER" }, 
                    new List<string> { "ERP_GetAllTaxInvoices", "ERP_GetTaxInvoiceById", "ERP_CreateTaxInvoice", "ERP_CreateTaxInvoiceDetail", "ERP_UpdateTaxInvoice", "ERP_DeleteTaxInvoice", "ERP_GetTaxInvoicePrintData", "ERP_CheckInvoiceLock", "ERP_LockInvoice", "ERP_UnlockInvoice" })
            };
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
                    dc.definition AS DefaultDefinition
                FROM sys.columns c
                INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
                LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
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
            
            // Build column definitions
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
                else if (typeName == "float" || typeName == "real")
                {
                    colDef.Append(typeName.ToUpper());
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
                    string cleanedDefaultDef = defaultDef.Trim();
                    if (cleanedDefaultDef.StartsWith("(") && cleanedDefaultDef.EndsWith(")"))
                        cleanedDefaultDef = cleanedDefaultDef.Substring(1, cleanedDefaultDef.Length - 2);
                    
                    if (cleanedDefaultDef.Contains("GETDATE") || cleanedDefaultDef.Contains("GETUTCDATE"))
                        colDef.Append($" DEFAULT {cleanedDefaultDef}");
                    else if (!string.IsNullOrEmpty(cleanedDefaultDef))
                        colDef.Append($" DEFAULT {cleanedDefaultDef}");
                }
                
                columns.Add(colDef.ToString());
            }
            
            sb.AppendLine(string.Join("," + Environment.NewLine, columns));
            sb.AppendLine($"    );");
            
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
                        string indexName = reader["IndexName"].ToString() ?? "";
                        string columnNames = reader["ColumnNames"].ToString() ?? "";
                        if (!string.IsNullOrEmpty(columnNames))
                        {
                            sb.AppendLine($"    ALTER TABLE [{schemaName}].[{tableName}]");
                            sb.AppendLine($"    ADD CONSTRAINT [{indexName}] PRIMARY KEY ({columnNames});");
                        }
                    }
                }
            }
            
            sb.AppendLine($"END");
            
            return sb.ToString();
        }
        
        static void ExtractStoredProcedure(SqlConnection connection, string schemaName, string procName, string sprocsPath, string controllerPath)
        {
            string query = @"
                SELECT OBJECT_DEFINITION(OBJECT_ID(@ProcName)) AS ProcedureDefinition;
            ";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProcName", $"{schemaName}.{procName}");
                var procDef = command.ExecuteScalar()?.ToString();
                
                if (string.IsNullOrEmpty(procDef))
                {
                    Console.WriteLine($"    Warning: Stored procedure {procName} not found");
                    return;
                }
                
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
                
                sb.Append(procDef);
                
                // Save to both locations
                string filePath = Path.Combine(sprocsPath, $"{procName}.sql");
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                
                string controllerFilePath = Path.Combine(controllerPath, $"{procName}.sql");
                File.WriteAllText(controllerFilePath, sb.ToString(), Encoding.UTF8);
            }
        }
    }
}

