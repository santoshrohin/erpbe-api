using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ErpBE.Tests.Integration;

/// <summary>
/// Deploys database schema (tables and stored procedures) to test container database
/// </summary>
public static class DatabaseSchemaDeployer
{
    private static bool _usedEntireDatabaseScript = false;
    
    /// <summary>
    /// Gets whether the entire database script was used
    /// </summary>
    public static bool UsedEntireDatabaseScript => _usedEntireDatabaseScript;
    
    /// <summary>
    /// Creates all required tables using the entire database script
    /// </summary>
    public static async Task CreateTablesAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Try to use entire database script first
        var entireDbScriptPath = GetEntireDatabaseScriptPath();
        if (!string.IsNullOrEmpty(entireDbScriptPath) && File.Exists(entireDbScriptPath))
        {
            Console.WriteLine($"---- Using entire database script: {Path.GetFileName(entireDbScriptPath)}");
            _usedEntireDatabaseScript = true;
            await ExecuteEntireDatabaseScriptAsync(connection, entireDbScriptPath);
        }
        else
        {
            // Fallback to individual table scripts
            _usedEntireDatabaseScript = false;
            Console.WriteLine($"---- Entire database script not found, using individual table scripts");
            var tables = GetTableCreationScripts();
            
            foreach (var tableScript in tables)
            {
                try
                {
                    // Skip empty table scripts (tables with no columns)
                    if (string.IsNullOrWhiteSpace(tableScript) || 
                        (tableScript.Contains("CREATE TABLE") && 
                         (tableScript.Contains("(\n\n    );") || 
                          tableScript.Contains("(\r\n\r\n    );") ||
                          tableScript.Contains("(\n    );") ||
                          tableScript.Contains("(\r\n    );"))))
                    {
                        Console.WriteLine($"---- Skipping empty table script (no columns defined)");
                        continue;
                    }
                    
                    var command = connection.CreateCommand();
                    command.CommandText = tableScript;
                    command.CommandTimeout = 60;
                    await command.ExecuteNonQueryAsync();
                }
                catch (SqlException ex) when (ex.Message.Contains("already exists") || ex.Number == 2714)
                {
                    // Table already exists, skip
                    Console.WriteLine($"---- Table already exists, skipping...");
                }
                catch (SqlException ex) when (ex.Message.Contains("Incorrect syntax") || ex.Number == 102)
                {
                    // Syntax error - log and skip
                    Console.WriteLine($"---- Syntax error in table script: {ex.Message}");
                    Console.WriteLine($"---- Skipping problematic table script");
                }
            }
        }
        
        // After creating tables, seed test data (required for login tests)
        Console.WriteLine("---- Seeding test data...");
        await SeedTestDataAsync(connectionString);
        
        // After creating tables, ensure TestUser password is correct
        await EnsureTestUserPasswordAsync(connectionString);
        
        // Create indexes for AUDIT_TRAIL after table creation
        await CreateAuditTrailIndexesAsync(connectionString);
    }
    
    /// <summary>
    /// Gets the path to the entire database script
    /// </summary>
    private static string GetEntireDatabaseScriptPath()
    {
        // Try multiple possible paths - looking for EntireDatabaseScript.sql
        var possiblePaths = new[]
        {
            // Relative paths from test project
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "Database_Scripts", "EntireDatabase", "EntireDatabaseScript.sql"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Database_Scripts", "EntireDatabase", "EntireDatabaseScript.sql"),
            // Absolute paths
            @"D:\Santosh\Work\Projects\WebBased\API\Database_Scripts\EntireDatabase\EntireDatabaseScript.sql",
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "Database_Scripts", "EntireDatabase", "EntireDatabaseScript.sql")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Database_Scripts", "EntireDatabase", "EntireDatabaseScript.sql"))
        };
        
        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                Console.WriteLine($"---- Found entire database script at: {fullPath}");
                return fullPath;
            }
        }
        
        Console.WriteLine($"---- Entire database script not found. Searched paths:");
        foreach (var path in possiblePaths)
        {
            Console.WriteLine($"----   {Path.GetFullPath(path)}");
        }
        
        return string.Empty;
    }
    
    /// <summary>
    /// Executes the entire database script
    /// </summary>
    private static async Task ExecuteEntireDatabaseScriptAsync(SqlConnection connection, string scriptPath)
    {
        var scriptContent = await File.ReadAllTextAsync(scriptPath);
        
        // Remove USE statements
        scriptContent = System.Text.RegularExpressions.Regex.Replace(
            scriptContent,
            @"^\s*USE\s+\[?[^\]]+\]?\s*;?\s*$",
            "",
            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Fix VARCHAR(-1) to VARCHAR(MAX) - SQL Server uses -1 internally but we need MAX for compatibility
        scriptContent = scriptContent.Replace("VARCHAR(-1)", "VARCHAR(MAX)");
        scriptContent = scriptContent.Replace("NVARCHAR(-1)", "NVARCHAR(MAX)");
        
        // Convert ALTER PROCEDURE to CREATE OR ALTER PROCEDURE for idempotency
        scriptContent = System.Text.RegularExpressions.Regex.Replace(
            scriptContent,
            @"ALTER\s+PROCEDURE",
            "CREATE OR ALTER PROCEDURE",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Split by GO statements - handle various line ending combinations
        var batches = System.Text.RegularExpressions.Regex.Split(
            scriptContent,
            @"\r?\n\s*GO\s*\r?\n",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        var totalBatches = batches.Length;
        var currentBatch = 0;
        var successCount = 0;
        var skipCount = 0;
        var errorCount = 0;
        
        Console.WriteLine($"---- Executing entire database script: {totalBatches} batches found");
        
        foreach (var batch in batches)
        {
            currentBatch++;
            var trimmedBatch = batch.Trim();
            
            if (string.IsNullOrWhiteSpace(trimmedBatch))
                continue;
            
            // Skip comments-only batches
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmedBatch, @"^(\s*--.*|\s*/\*.*\*/\s*)+$", System.Text.RegularExpressions.RegexOptions.Singleline))
                continue;
            
            // Skip empty CREATE TABLE statements
            if (trimmedBatch.Contains("CREATE TABLE") && 
                (trimmedBatch.Contains("(\n\n    );") || trimmedBatch.Contains("(\r\n\r\n    );")))
            {
                Console.WriteLine($"---- Skipping empty table definition (batch {currentBatch}/{totalBatches})");
                skipCount++;
                continue;
            }
            
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = trimmedBatch;
                command.CommandTimeout = 180; // Longer timeout for large scripts
                await command.ExecuteNonQueryAsync();
                successCount++;
                
                // Show progress every 20 batches or at milestones
                if (currentBatch % 20 == 0 || currentBatch == totalBatches)
                {
                    Console.WriteLine($"---- Progress: {currentBatch}/{totalBatches} batches (Success: {successCount}, Skipped: {skipCount}, Errors: {errorCount})");
                }
            }
            catch (SqlException ex) when (ex.Message.Contains("already exists") || ex.Number == 2714 || ex.Number == 2715 || ex.Number == 1913)
            {
                // Object already exists, skip
                skipCount++;
                if (currentBatch % 50 == 0 || currentBatch == totalBatches)
                {
                    Console.WriteLine($"---- Object already exists, skipping batch {currentBatch}/{totalBatches}");
                }
            }
            catch (SqlException ex) when (ex.Message.Contains("Incorrect syntax") || ex.Number == 102)
            {
                // Syntax error - log and continue
                errorCount++;
                Console.WriteLine($"---- Syntax error in batch {currentBatch}/{totalBatches}: {ex.Message}");
                var preview = trimmedBatch.Length > 150 ? trimmedBatch.Substring(0, 150) + "..." : trimmedBatch;
                Console.WriteLine($"---- Batch preview: {preview}");
            }
            catch (SqlException ex)
            {
                // Other SQL errors - log and continue
                errorCount++;
                Console.WriteLine($"---- Error in batch {currentBatch}/{totalBatches}: {ex.Message} (Number: {ex.Number})");
                if (errorCount <= 5) // Only show first 5 errors in detail
                {
                    var preview = trimmedBatch.Length > 150 ? trimmedBatch.Substring(0, 150) + "..." : trimmedBatch;
                    Console.WriteLine($"---- Batch preview: {preview}");
                }
            }
        }
        
        Console.WriteLine($"---- Completed executing entire database script: {totalBatches} batches (Success: {successCount}, Skipped: {skipCount}, Errors: {errorCount})");
    }

    /// <summary>
    /// Creates indexes for AUDIT_TRAIL table
    /// </summary>
    private static async Task CreateAuditTrailIndexesAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var indexScripts = new[]
        {
            "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AUDIT_TRAIL_TABLE_RECORD' AND object_id = OBJECT_ID(N'[dbo].[AUDIT_TRAIL]')) CREATE NONCLUSTERED INDEX [IX_AUDIT_TRAIL_TABLE_RECORD] ON [dbo].[AUDIT_TRAIL] ([TABLE_NAME], [RECORD_ID])",
            "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AUDIT_TRAIL_CREATED_DATE' AND object_id = OBJECT_ID(N'[dbo].[AUDIT_TRAIL]')) CREATE NONCLUSTERED INDEX [IX_AUDIT_TRAIL_CREATED_DATE] ON [dbo].[AUDIT_TRAIL] ([CREATED_DATE])",
            "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AUDIT_TRAIL_ACTION_TYPE' AND object_id = OBJECT_ID(N'[dbo].[AUDIT_TRAIL]')) CREATE NONCLUSTERED INDEX [IX_AUDIT_TRAIL_ACTION_TYPE] ON [dbo].[AUDIT_TRAIL] ([ACTION_TYPE])",
            "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AUDIT_TRAIL_EntityName_EntityId' AND object_id = OBJECT_ID(N'[dbo].[AUDIT_TRAIL]')) CREATE NONCLUSTERED INDEX [IX_AUDIT_TRAIL_EntityName_EntityId] ON [dbo].[AUDIT_TRAIL] ([EntityName], [EntityId])",
            "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AUDIT_TRAIL_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[AUDIT_TRAIL]')) CREATE NONCLUSTERED INDEX [IX_AUDIT_TRAIL_Timestamp] ON [dbo].[AUDIT_TRAIL] ([Timestamp])"
        };
        
        foreach (var script in indexScripts)
        {
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = script;
                command.CommandTimeout = 60;
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number == 1913 || ex.Message.Contains("already exists"))
            {
                // Index already exists, skip
            }
        }
    }

    /// <summary>
    /// Seeds test data required for tests (COMPANY_MASTER, USER_MASTER, ROLES, UserRoles)
    /// </summary>
    public static async Task SeedTestDataAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        Console.WriteLine("---- Seeding test data...");

        // Seed COMPANY_MASTER first (required for other tables)
        await SeedCompanyMasterAsync(connection);

        // Seed ROLES (required before UserRoles)
        await SeedRolesAsync(connection);

        // Seed USER_MASTER (password will be set by EnsureTestUserPasswordAsync)
        await SeedUserMasterAsync(connection);

        // Assign Admin role to TestUser
        await AssignAdminRoleToTestUserAsync(connection);

        // Seed USER_RIGHT permissions for TestUser
        await SeedUserRightAsync(connection);

        Console.WriteLine("---- Test data seeded successfully");

        // Seed master data required by TaxInvoice integration tests
        await SeedMasterTestDataAsync(connectionString);
    }

    /// <summary>
    /// Seeds COMPANY_MASTER table
    /// </summary>
    private static async Task SeedCompanyMasterAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[COMPANY_MASTER]') AND type = N'U') RETURN;

            IF NOT EXISTS (SELECT 1 FROM [dbo].[COMPANY_MASTER] WHERE CM_ID = 1)
            BEGIN
                -- Use IDENTITY_INSERT if CM_ID is an identity column; otherwise insert directly
                IF EXISTS (SELECT 1 FROM sys.columns
                           WHERE object_id = OBJECT_ID('COMPANY_MASTER')
                             AND name = 'CM_ID'
                             AND is_identity = 1)
                BEGIN
                    SET IDENTITY_INSERT [dbo].[COMPANY_MASTER] ON;
                    INSERT INTO [dbo].[COMPANY_MASTER] ([CM_ID], [CM_CODE], [CM_NAME], [CM_EMAILID], [CM_OPENING_DATE], [CM_CLOSING_DATE], [CM_ACTIVE_IND])
                    VALUES (1, -2147483641, 'Test Company', 'test@company.com', '2024-01-01', '2024-12-31', 1);
                    SET IDENTITY_INSERT [dbo].[COMPANY_MASTER] OFF;
                END
                ELSE
                BEGIN
                    INSERT INTO [dbo].[COMPANY_MASTER] ([CM_ID], [CM_CODE], [CM_NAME], [CM_EMAILID], [CM_OPENING_DATE], [CM_CLOSING_DATE], [CM_ACTIVE_IND])
                    VALUES (1, -2147483641, 'Test Company', 'test@company.com', '2024-01-01', '2024-12-31', 1);
                END
            END
            ELSE
            BEGIN
                -- Ensure the existing row is active
                UPDATE [dbo].[COMPANY_MASTER]
                SET [CM_ACTIVE_IND] = 1
                WHERE CM_ID = 1;
            END";
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("---- COMPANY_MASTER seeded");
    }
    
    /// <summary>
    /// Seeds ROLES table
    /// </summary>
    private static async Task SeedRolesAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            -- Seed Admin role
            IF NOT EXISTS (SELECT * FROM [dbo].[ROLES] WHERE RoleName = 'Admin')
            BEGIN
                -- Check if RoleId 1 exists
                IF NOT EXISTS (SELECT * FROM [dbo].[ROLES] WHERE RoleId = 1)
                BEGIN
                    SET IDENTITY_INSERT [dbo].[ROLES] ON;
                    INSERT INTO [dbo].[ROLES] ([RoleId], [RoleName], [IsActive]) VALUES (1, 'Admin', 1);
                    SET IDENTITY_INSERT [dbo].[ROLES] OFF;
                END
                ELSE
                BEGIN
                    -- Insert without specifying RoleId (let identity generate it)
                    INSERT INTO [dbo].[ROLES] ([RoleName], [IsActive]) VALUES ('Admin', 1);
                END
            END
            ELSE
            BEGIN
                -- Ensure Admin role is active
                UPDATE [dbo].[ROLES]
                SET [IsActive] = 1
                WHERE RoleName = 'Admin';
            END";
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("---- ROLES seeded");
    }
    
    /// <summary>
    /// Seeds USER_MASTER table
    /// </summary>
    private static async Task SeedUserMasterAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            -- Seed TestUser (password will be set by EnsureTestUserPasswordAsync)
            -- Note: UM_CODE is IDENTITY(-2147483648,1), so we don't specify it
            IF NOT EXISTS (SELECT * FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser')
            BEGIN
                INSERT INTO [dbo].[USER_MASTER] ([UM_USERNAME], [UM_PASSWORD], [UM_NAME], [UM_EMAIL], [UM_CM_ID], [IS_ACTIVE], [UM_IS_ADMIN], [ES_DELETE])
                VALUES ('TestUser', '', 'Test User', 'test@test.com', 1, 1, 1, 0);
            END
            ELSE
            BEGIN
                -- Update existing TestUser to ensure it's active and has correct company
                UPDATE [dbo].[USER_MASTER]
                SET [IS_ACTIVE] = 1,
                    [UM_IS_ADMIN] = 1,
                    [ES_DELETE] = 0,
                    [UM_CM_ID] = 1,
                    [UM_NAME] = 'Test User',
                    [UM_EMAIL] = 'test@test.com'
                WHERE UM_USERNAME = 'TestUser';
            END";
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("---- USER_MASTER seeded");
    }
    
    /// <summary>
    /// Assigns Admin role to TestUser
    /// </summary>
    private static async Task AssignAdminRoleToTestUserAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            -- Assign Admin role to TestUser
            -- First, get the TestUser's UM_CODE and Admin's RoleId
            DECLARE @UserId INT;
            DECLARE @RoleId INT;
            
            SELECT @UserId = UM_CODE FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser';
            SELECT @RoleId = RoleId FROM [dbo].[ROLES] WHERE RoleName = 'Admin';
            
            -- Only proceed if both exist
            IF @UserId IS NOT NULL AND @RoleId IS NOT NULL
            BEGIN
                -- Check if the role assignment already exists
                IF NOT EXISTS (SELECT * FROM [dbo].[UserRoles] WHERE UserId = @UserId AND RoleId = @RoleId)
                BEGIN
                    INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId], [IsActive])
                    VALUES (@UserId, @RoleId, 1);
                END
                ELSE
                BEGIN
                    -- Ensure the role assignment is active
                    UPDATE [dbo].[UserRoles]
                    SET [IsActive] = 1
                    WHERE UserId = @UserId AND RoleId = @RoleId;
                END
            END";
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("---- Admin role assigned to TestUser");
    }
    
    /// <summary>
    /// Seeds USER_RIGHT with full permissions for TestUser across all legacy modules.
    /// Required for ERP_GetUserPermissions to return data in integration tests.
    /// </summary>
    private static async Task SeedUserRightAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            DECLARE @UserId INT;
            SELECT @UserId = UM_CODE FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser';
            IF @UserId IS NOT NULL AND EXISTS (SELECT 1 FROM sys.objects WHERE name = 'USER_RIGHT' AND type = 'U')
            BEGIN
                DELETE FROM [dbo].[USER_RIGHT] WHERE UR_UM_CODE = @UserId;
                INSERT INTO [dbo].[USER_RIGHT] (UR_UM_CODE, UR_SM_CODE, UR_RIGHTS, UR_IS_DELETE)
                VALUES
                    (@UserId, 72,  '1111111', 0),
                    (@UserId, 73,  '1111111', 0),
                    (@UserId, 74,  '1111111', 0),
                    (@UserId, 75,  '1111111', 0),
                    (@UserId, 76,  '1111111', 0),
                    (@UserId, 77,  '1111111', 0),
                    (@UserId, 99,  '1111111', 0),
                    (@UserId, 106, '1111111', 0);
            END";
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("---- USER_RIGHT seeded for TestUser");
    }

    /// <summary>
    /// Ensures TestUser has the correct encrypted password
    /// </summary>
    private static async Task EnsureTestUserPasswordAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Calculate encrypted password for 'Test@123' using LegacyEncryption algorithm
        var encryptedPassword = EncryptPassword("Test@123");
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            -- Ensure TestUser exists and has correct password
            IF EXISTS (SELECT * FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser')
            BEGIN
                UPDATE [dbo].[USER_MASTER]
                SET UM_PASSWORD = @Password,
                    IS_ACTIVE = 1,
                    UM_IS_ADMIN = 1,
                    ES_DELETE = 0,
                    UM_CM_ID = 1
                WHERE UM_USERNAME = 'TestUser';
            END
            ELSE
            BEGIN
                -- Insert TestUser if it doesn't exist (shouldn't happen if SeedTestDataAsync ran correctly)
                INSERT INTO [dbo].[USER_MASTER] ([UM_USERNAME], [UM_PASSWORD], [UM_NAME], [UM_EMAIL], [UM_CM_ID], [IS_ACTIVE], [UM_IS_ADMIN], [ES_DELETE])
                VALUES ('TestUser', @Password, 'Test User', 'test@test.com', 1, 1, 1, 0);
            END";
        command.Parameters.AddWithValue("@Password", encryptedPassword);
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("---- TestUser password set successfully");
    }
    
    /// <summary>
    /// Seeds master data required by TaxInvoice integration tests.
    /// Safe to run against shared/production DBs: uses IF NOT EXISTS guards and IDENTITY_INSERT.
    /// Adds ITEM_MASTER (I_CODE 1 &amp; 2), ITEM_UNIT_MASTER (I_UOM_CODE 1), PARTY_MASTER (P_CODE 1),
    /// CUSTPO_MASTER (CPOM_CODE 1), and CUSTPO_DETAIL for item 1 in PO 1.
    /// </summary>
    public static async Task SeedMasterTestDataAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        Console.WriteLine("---- Seeding TaxInvoice master test data...");
        try { await SeedCompanyMasterAsync(connection); }
        catch (Exception ex) { Console.WriteLine($"---- COMPANY_MASTER seed skipped: {ex.Message}"); }
        await SeedItemMasterAsync(connection);
        await SeedItemUnitMasterAsync(connection);
        await SeedPartyMasterAsync(connection);
        await SeedCustPoMasterAsync(connection);
        Console.WriteLine("---- TaxInvoice master test data seeded");
    }

    private static async Task SeedItemMasterAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandTimeout = 60;
        command.CommandText = @"
            -- Ensure test items exist so ERP_GetTaxInvoiceItemDetails can return rows
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ITEM_MASTER]') AND type = N'U') RETURN;
            IF NOT EXISTS (SELECT 1 FROM [dbo].[ITEM_MASTER] WHERE I_CODE = 1)
            BEGIN
                SET IDENTITY_INSERT [dbo].[ITEM_MASTER] ON;
                INSERT INTO [dbo].[ITEM_MASTER] ([I_CODE], [I_NAME], [I_CODENO], [ES_DELETE])
                VALUES (1, 'Test Item 1', 'TI-001', 0);
                SET IDENTITY_INSERT [dbo].[ITEM_MASTER] OFF;
            END
            IF NOT EXISTS (SELECT 1 FROM [dbo].[ITEM_MASTER] WHERE I_CODE = 2)
            BEGIN
                SET IDENTITY_INSERT [dbo].[ITEM_MASTER] ON;
                INSERT INTO [dbo].[ITEM_MASTER] ([I_CODE], [I_NAME], [I_CODENO], [ES_DELETE])
                VALUES (2, 'Test Item 2', 'TI-002', 0);
                SET IDENTITY_INSERT [dbo].[ITEM_MASTER] OFF;
            END";
        try { await command.ExecuteNonQueryAsync(); Console.WriteLine("---- ITEM_MASTER seeded"); }
        catch (Exception ex) { Console.WriteLine($"---- ITEM_MASTER seed skipped: {ex.Message}"); }
    }

    private static async Task SeedItemUnitMasterAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandTimeout = 60;
        // I_UOM_CM_COMP_ID is the actual column name (not I_UOM_CM_ID)
        command.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ITEM_UNIT_MASTER]') AND type = N'U') RETURN;
            IF NOT EXISTS (SELECT 1 FROM [dbo].[ITEM_UNIT_MASTER] WHERE I_UOM_CODE = 1)
            BEGIN
                SET IDENTITY_INSERT [dbo].[ITEM_UNIT_MASTER] ON;
                INSERT INTO [dbo].[ITEM_UNIT_MASTER] ([I_UOM_CODE], [I_UOM_NAME], [I_UOM_CM_COMP_ID], [ES_DELETE])
                VALUES (1, 'Nos', 1, 0);
                SET IDENTITY_INSERT [dbo].[ITEM_UNIT_MASTER] OFF;
            END";
        try { await command.ExecuteNonQueryAsync(); Console.WriteLine("---- ITEM_UNIT_MASTER seeded"); }
        catch (Exception ex) { Console.WriteLine($"---- ITEM_UNIT_MASTER seed skipped: {ex.Message}"); }
    }

    private static async Task SeedPartyMasterAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandTimeout = 60;
        // P_TYPE, P_NAME, P_CONTACT, P_ADD1 are NOT NULL in the production schema
        command.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PARTY_MASTER]') AND type = N'U') RETURN;
            IF NOT EXISTS (SELECT 1 FROM [dbo].[PARTY_MASTER] WHERE P_CODE = 1)
            BEGIN
                SET IDENTITY_INSERT [dbo].[PARTY_MASTER] ON;
                INSERT INTO [dbo].[PARTY_MASTER]
                    ([P_CODE], [P_CM_COMP_ID], [P_TYPE], [P_NAME], [P_CONTACT], [P_ADD1], [ES_DELETE])
                VALUES (1, 1, 1, 'Test Customer 1', 'Test Contact', 'Test Address', 0);
                SET IDENTITY_INSERT [dbo].[PARTY_MASTER] OFF;
            END";
        try { await command.ExecuteNonQueryAsync(); Console.WriteLine("---- PARTY_MASTER seeded"); }
        catch (Exception ex) { Console.WriteLine($"---- PARTY_MASTER seed skipped: {ex.Message}"); }
    }

    private static async Task SeedCustPoMasterAsync(SqlConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandTimeout = 60;
        command.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUSTPO_MASTER]') AND type = N'U') RETURN;
            IF NOT EXISTS (SELECT 1 FROM [dbo].[CUSTPO_MASTER] WHERE CPOM_CODE = 1)
            BEGIN
                SET IDENTITY_INSERT [dbo].[CUSTPO_MASTER] ON;
                INSERT INTO [dbo].[CUSTPO_MASTER]
                    ([CPOM_CODE], [CPOM_P_CODE], [CPOM_CM_COMP_ID], [CPOM_PONO], [CPOM_DATE], [ES_DELETE])
                VALUES (1, 1, 1, 'TEST-PO-001', GETDATE(), 0);
                SET IDENTITY_INSERT [dbo].[CUSTPO_MASTER] OFF;
            END
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUSTPO_DETAIL]') AND type = N'U') RETURN;
            IF NOT EXISTS (SELECT 1 FROM [dbo].[CUSTPO_DETAIL] WHERE CPOD_CPOM_CODE = 1 AND CPOD_I_CODE = 1)
            BEGIN
                INSERT INTO [dbo].[CUSTPO_DETAIL]
                    ([CPOD_CPOM_CODE], [CPOD_I_CODE], [CPOD_UOM_CODE], [CPOD_ORD_QTY], [CPOD_RATE])
                VALUES (1, 1, 1, 1000, 100);
            END";
        try { await command.ExecuteNonQueryAsync(); Console.WriteLine("---- CUSTPO_MASTER/DETAIL seeded"); }
        catch (Exception ex) { Console.WriteLine($"---- CUSTPO seed skipped: {ex.Message}"); }
    }

    /// <summary>
    /// Re-seeds test data after database reset (called after Respawner cleanup)
    /// </summary>
    public static async Task SeedTestDataAfterResetAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        Console.WriteLine("---- Re-seeding test data after reset...");
        
        // Seed COMPANY_MASTER first (required for other tables)
        await SeedCompanyMasterAsync(connection);
        
        // Seed ROLES (required before UserRoles)
        await SeedRolesAsync(connection);
        
        // Seed USER_MASTER (password will be set by EnsureTestUserPasswordAsync)
        await SeedUserMasterAsync(connection);
        
        // Assign Admin role to TestUser
        await AssignAdminRoleToTestUserAsync(connection);

        // Seed USER_RIGHT permissions for TestUser
        await SeedUserRightAsync(connection);

        // Ensure TestUser has the correct encrypted password
        await EnsureTestUserPasswordAsync(connectionString);

        // Re-seed TaxInvoice master test data (wiped by Respawner)
        await SeedMasterTestDataAsync(connectionString);

        Console.WriteLine("---- Test data re-seeded successfully");
    }

    /// <summary>
    /// Encrypts password using LegacyEncryption algorithm
    /// </summary>
    private static string EncryptPassword(string password)
    {
        var result = new StringBuilder();
        int pos = 0;
        
        foreach (char c in password)
        {
            int ascii = (int)c;
            int encrypted = (ascii * 20) / 2 - 100;
            
            if (pos == 0)
            {
                result.Append(encrypted);
                pos++;
            }
            else
            {
                result.Append("-").Append(encrypted);
            }
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Deploys all stored procedures from the Database_Scripts/StoredProcedures directory
    /// </summary>
    public static async Task DeployStoredProceduresAsync(string connectionString)
    {
        // Try multiple paths to find the stored procedures directory
        var possiblePaths = new[]
        {
            // Path from test output directory
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Database_Scripts", "StoredProcedures"),
            // Path from test project directory
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Database_Scripts", "StoredProcedures"),
            // Absolute path from workspace root
            Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? "", "Database_Scripts", "StoredProcedures"),
            // Path relative to test assembly location
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "..", "..", "..", "..", "Database_Scripts", "StoredProcedures")
        };

        string? scriptPath = null;
        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                scriptPath = fullPath;
                break;
            }
        }

        if (string.IsNullOrEmpty(scriptPath) || !Directory.Exists(scriptPath))
        {
            // Last resort: try to find it from the current working directory
            var currentDir = Directory.GetCurrentDirectory();
            var rootDir = currentDir;
            for (int i = 0; i < 5; i++)
            {
                var testPath = Path.Combine(rootDir, "Database_Scripts", "StoredProcedures");
                if (Directory.Exists(testPath))
                {
                    scriptPath = testPath;
                    break;
                }
                rootDir = Directory.GetParent(rootDir)?.FullName ?? "";
                if (string.IsNullOrEmpty(rootDir)) break;
            }
        }

        if (string.IsNullOrEmpty(scriptPath) || !Directory.Exists(scriptPath))
        {
            throw new DirectoryNotFoundException(
                $"Stored procedures directory not found. Searched in:\n" +
                string.Join("\n", possiblePaths.Select(p => $"  - {Path.GetFullPath(p)}")));
        }

        Console.WriteLine($"---- Using stored procedures path: {scriptPath}");

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Get all SQL files recursively
        var sqlFiles = Directory.GetFiles(scriptPath, "*.sql", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Deploy") && 
                   !f.EndsWith("DeployAllCustomerTypeMasterProcedures.sql") && 
                   !f.EndsWith("DeployAllSoTypeMasterProcedures.sql") &&
                   !f.Contains("_FIXED") &&
                   !f.Contains("_CORRECTED") &&
                   !f.Contains("_FINAL") &&
                   !f.Contains("_V2"))
            .OrderBy(f => f)
            .ToList();
        
        // Ensure SP_VerifyLogin is deployed first (it's needed for login tests)
        var verifyLoginFile = sqlFiles.FirstOrDefault(f => f.EndsWith("SP_VerifyLogin.sql"));
        if (verifyLoginFile != null)
        {
            sqlFiles.Remove(verifyLoginFile);
            sqlFiles.Insert(0, verifyLoginFile);
        }
        
        // Ensure SP_GetUserRoles is deployed early (it's needed for login)
        // Check both Database_Scripts and ErpBE.Database directories
        var getUserRolesFile = sqlFiles.FirstOrDefault(f => 
            f.EndsWith("SP_GetUserRoles.sql") || 
            f.EndsWith("SP_GetUserRolesByUserId.sql") ||
            f.Contains("SP_GetUserRoles"));
        if (getUserRolesFile != null && !sqlFiles.Take(5).Any(f => f == getUserRolesFile))
        {
            sqlFiles.Remove(getUserRolesFile);
            sqlFiles.Insert(1, getUserRolesFile);
        }
        
        Console.WriteLine($"---- Found {sqlFiles.Count} stored procedure files to deploy");

        foreach (var sqlFile in sqlFiles)
        {
            try
            {
                var sqlContent = await File.ReadAllTextAsync(sqlFile);
                
                // Split by GO statements
                var batches = sqlContent.Split(new[] { "\r\nGO\r\n", "\r\nGO\n", "\nGO\r\n", "\nGO\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var batch in batches)
                {
                    var trimmedBatch = batch.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedBatch))
                        continue;
                    
                    // Remove USE statements that reference wrong database
                    trimmedBatch = System.Text.RegularExpressions.Regex.Replace(
                        trimmedBatch,
                        @"^\s*USE\s+\[?[^\]]+\]?\s*;?\s*$",
                        "",
                        System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    // Also remove USE statements at the beginning of the batch
                    trimmedBatch = System.Text.RegularExpressions.Regex.Replace(
                        trimmedBatch,
                        @"^\s*USE\s+[^\s;]+;?\s*",
                        "",
                        System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    // Convert ALTER PROCEDURE to CREATE OR ALTER PROCEDURE
                    trimmedBatch = System.Text.RegularExpressions.Regex.Replace(
                        trimmedBatch,
                        @"^\s*ALTER\s+PROCEDURE\s+",
                        "CREATE OR ALTER PROCEDURE ",
                        System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    trimmedBatch = trimmedBatch.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedBatch))
                        continue;

                    var command = connection.CreateCommand();
                    command.CommandText = trimmedBatch;
                    command.CommandTimeout = 60;
                    
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) when (ex.Number == 2714 || ex.Number == 2715)
                    {
                        // Object already exists, skip
                        Console.WriteLine($"---- Stored procedure already exists, skipping: {Path.GetFileName(sqlFile)}");
                    }
                }
                
                Console.WriteLine($"---- Deployed: {Path.GetFileName(sqlFile)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"---- Error deploying {Path.GetFileName(sqlFile)}: {ex.Message}");
                // Continue with other files
            }
        }
    }

    /// <summary>
    /// Gets table creation scripts for all required tables from actual database scripts
    /// </summary>
    private static List<string> GetTableCreationScripts()
    {
        // Get the base path - Database_Scripts is one level up from API folder
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "Database_Scripts");
        var tablesPath = Path.Combine(basePath, "Tables");
        
        // If tables don't exist in that location, try relative to API folder
        if (!Directory.Exists(tablesPath))
        {
            basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Database_Scripts");
            tablesPath = Path.Combine(basePath, "Tables");
        }
        
        // If still not found, fall back to generated scripts
        if (!Directory.Exists(tablesPath))
        {
            Console.WriteLine($"---- Warning: Tables directory not found at {tablesPath}, using generated scripts");
            return GetGeneratedTableScripts();
        }
        
        // Define tables in dependency order (foreign keys must be created after parent tables)
        var tableOrder = new[]
        {
            "COMPANY_MASTER",
            "FINANCIAL_YEAR_MASTER",
            "AREA_MASTER",
            "CUSTOMER_TYPE_MASTER",
            "ITEM_CATEGORY_MASTER",
            "ITEM_UNIT_MASTER",
            "SO_TYPE_MASTER",
            "PARTY_MASTER",
            "ITEM_MASTER",
            "UNIT_MASTER",
            "USER_MASTER",
            "USER_RIGHT",
            "USER_REFRESH_TOKEN",
            "ROLES",
            "UserRoles",
            "CUSTPO_MASTER",
            "CUSTPO_DETAIL",
            "TAX_INVOICE_MASTER",
            "TAX_INVOICE_DETAIL",
            "INVOICE_MASTER",
            "AUDIT_TRAIL",
            "AUDIT_CONFIGURATION",
            "Logs"
        };
        
        var scripts = new List<string>();
        
        foreach (var tableName in tableOrder)
        {
            var tableFile = Path.Combine(tablesPath, $"{tableName}.sql");
            if (File.Exists(tableFile))
            {
                var script = File.ReadAllText(tableFile);
                scripts.Add(script);
                Console.WriteLine($"---- Loaded table script: {tableName}");
            }
            else
            {
                Console.WriteLine($"---- Warning: Table script not found: {tableName}, using generated script");
                // Add generated script for this table if available
                var generatedScript = GetGeneratedTableScript(tableName);
                if (!string.IsNullOrEmpty(generatedScript))
                {
                    scripts.Add(generatedScript);
                }
            }
        }
        
        // Add test data seeding after all tables are created
        scripts.Add(GetTestDataSeedingScript());
        
        return scripts;
    }
    
    /// <summary>
    /// Gets generated table script for a specific table (fallback)
    /// </summary>
    private static string GetGeneratedTableScript(string tableName)
    {
        // Return empty if we want to skip generated scripts
        return string.Empty;
    }
    
    /// <summary>
    /// Gets test data seeding script
    /// </summary>
    private static string GetTestDataSeedingScript()
    {
        return @"
            -- Seed test data for COMPANY_MASTER (CM_ID=1, CM_CODE=-2147483641)
            -- Note: CM_ID is not IDENTITY, CM_CODE is NOT NULL
            IF NOT EXISTS (SELECT * FROM [dbo].[COMPANY_MASTER] WHERE CM_ID = 1 OR CM_CODE = -2147483641)
            BEGIN
                INSERT INTO [dbo].[COMPANY_MASTER] ([CM_CODE], [CM_ID], [CM_NAME], [CM_EMAILID], [CM_OPENING_DATE], [CM_CLOSING_DATE], [CM_ACTIVE_IND])
                VALUES (-2147483641, 1, 'Test Company 1', 'test@company.com', '2024-01-01', '2024-12-31', 1);
            END
            
            -- Seed test data for USER_MASTER (TestUser - password will be set by EnsureTestUserPasswordAsync)
            -- Note: UM_CODE is IDENTITY(-2147483648,1), so we don't specify it - let it auto-generate
            IF NOT EXISTS (SELECT * FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser')
            BEGIN
                INSERT INTO [dbo].[USER_MASTER] ([UM_USERNAME], [UM_PASSWORD], [UM_NAME], [UM_EMAIL], [UM_CM_ID], [IS_ACTIVE], [UM_IS_ADMIN], [ES_DELETE])
                VALUES ('TestUser', '', 'Test User', 'test@test.com', 1, 1, 1, 0);
            END
            
            -- Seed test data for ROLES
            IF NOT EXISTS (SELECT * FROM [dbo].[ROLES] WHERE RoleName = 'Admin')
            BEGIN
                SET IDENTITY_INSERT [dbo].[ROLES] ON;
                INSERT INTO [dbo].[ROLES] ([RoleId], [RoleName], [IsActive]) VALUES (1, 'Admin', 1);
                SET IDENTITY_INSERT [dbo].[ROLES] OFF;
            END
            
            -- Assign Admin role to TestUser (UserRoles table doesn't have Id column, it's a composite PK)
            -- Use UM_CODE from USER_MASTER (which is the identity column)
            IF NOT EXISTS (SELECT * FROM [dbo].[UserRoles] ur 
                INNER JOIN [dbo].[USER_MASTER] um ON ur.UserId = um.UM_CODE
                INNER JOIN [dbo].[ROLES] r ON ur.RoleId = r.RoleId
                WHERE um.UM_USERNAME = 'TestUser' AND r.RoleName = 'Admin')
            BEGIN
                INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId], [IsActive])
                SELECT um.UM_CODE, r.RoleId, 1
                FROM [dbo].[USER_MASTER] um
                CROSS JOIN [dbo].[ROLES] r
                WHERE um.UM_USERNAME = 'TestUser' AND r.RoleName = 'Admin'
                  AND NOT EXISTS (SELECT 1 FROM [dbo].[UserRoles] ur2 WHERE ur2.UserId = um.UM_CODE AND ur2.RoleId = r.RoleId);
            END
        ";
    }
    
    /// <summary>
    /// Fallback: Gets generated table creation scripts (original implementation)
    /// </summary>
    private static List<string> GetGeneratedTableScripts()
    {
        return new List<string>
        {
            // COMPANY_MASTER table
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[COMPANY_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[COMPANY_MASTER] (
                    [CM_ID] INT PRIMARY KEY IDENTITY(1,1),
                    [CM_CODE] INT NOT NULL,
                    [CM_NAME] NVARCHAR(255) NOT NULL,
                    [CM_EMAILID] NVARCHAR(255),
                    [CM_GST_NO] NVARCHAR(50),
                    [CM_ADDRESS1] NVARCHAR(MAX),
                    [CM_ADDRESS2] NVARCHAR(MAX),
                    [CM_ADDRESS3] NVARCHAR(MAX),
                    [CM_CITY] NVARCHAR(100),
                    [CM_STATE] NVARCHAR(100),
                    [CM_PIN] NVARCHAR(20),
                    [CM_PHONE] NVARCHAR(50),
                    [CM_FAX] NVARCHAR(50),
                    [CM_OPENING_DATE] DATETIME,
                    [CM_CLOSING_DATE] DATETIME,
                    [CM_ACTIVE_IND] BIT DEFAULT 1,
                    [CM_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [CM_MODIFIED_DATE] DATETIME DEFAULT GETDATE()
                )
                
                -- Insert test data (CM_ID=1, CM_CODE=-2147483641 for FinancialYearCode)
                -- Since CM_ID is IDENTITY, we need to set IDENTITY_INSERT ON to insert CM_ID=1
                IF NOT EXISTS (SELECT * FROM [dbo].[COMPANY_MASTER] WHERE CM_ID = 1)
                BEGIN
                    SET IDENTITY_INSERT [dbo].[COMPANY_MASTER] ON
                    INSERT INTO [dbo].[COMPANY_MASTER] ([CM_ID], [CM_CODE], [CM_NAME], [CM_EMAILID], [CM_OPENING_DATE], [CM_CLOSING_DATE], [CM_ACTIVE_IND])
                    VALUES (1, -2147483641, 'Test Company 1', 'test@company.com', '2024-01-01', '2024-12-31', 1)
                    SET IDENTITY_INSERT [dbo].[COMPANY_MASTER] OFF
                END
            END",

            // AREA_MASTER table
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AREA_MASTER]') AND type = N'U')
            BEGIN
                CREATE TABLE [dbo].[AREA_MASTER] (
                    [A_CODE]       INT IDENTITY(-2147483648,1) NOT NULL CONSTRAINT PK_AREA_MASTER PRIMARY KEY,
                    [A_U_CODE]     INT NULL,
                    [A_U_DATE]     DATETIME NULL,
                    [A_CM_COMP_ID] INT NULL,
                    [A_NO]         VARCHAR(10) NULL,
                    [A_DESC]       VARCHAR(50) NULL,
                    [ES_DELETE]    BIT NULL DEFAULT ((0)),
                    [MODIFY]       BIT NULL DEFAULT ((0))
                )
            END",

            // INVOICE_MASTER table (161 columns — only the core subset needed for tests)
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[INVOICE_MASTER]') AND type = N'U')
            BEGIN
                CREATE TABLE [dbo].[INVOICE_MASTER] (
                    [INM_CODE]        INT IDENTITY(-2147483648,1) NOT NULL CONSTRAINT PK_INVOICE_MASTER PRIMARY KEY,
                    [INM_CM_CODE]     INT NULL,
                    [INM_NO]          INT NULL,
                    [INM_DATE]        DATETIME NULL,
                    [INM_INVOICE_TYPE] TINYINT NULL,
                    [INM_TYPE]        VARCHAR(50) NULL,
                    [INM_P_CODE]      INT NULL,
                    [INM_CPOM_CODE]   INT NULL,
                    [INM_NET_AMT]     FLOAT NULL DEFAULT ((0)),
                    [INM_G_AMT]       FLOAT NULL DEFAULT ((0)),
                    [INM_ROUNDING_AMT] FLOAT NULL,
                    [INM_TAXABLE_AMT] FLOAT NULL,
                    [INM_ACCESSIBLE_AMT] FLOAT NULL,
                    [INM_STATE]       INT NULL,
                    [INM_HSN_CODE]    VARCHAR(50) NULL,
                    [INM_REMARK]      VARCHAR(255) NULL,
                    [ES_DELETE]       BIT NULL DEFAULT ((0)),
                    [MODIFY]          BIT NULL DEFAULT ((0))
                )
            END",

            // PARTY_MASTER table (Customer Master) - matches stored procedure schema
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PARTY_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[PARTY_MASTER] (
                    [P_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [P_CM_COMP_ID] INT NOT NULL,
                    [P_PARTY_CODE] INT NOT NULL,
                    [P_TYPE] INT DEFAULT 1,
                    [P_NAME] NVARCHAR(500) NOT NULL,
                    [P_CONTACT] NVARCHAR(75),
                    [P_ABBREVATION] NVARCHAR(20),
                    [P_VEND_CODE] NVARCHAR(30),
                    [P_ADD1] NVARCHAR(255),
                    [P_PHONE] NVARCHAR(50),
                    [P_MOB] NVARCHAR(55),
                    [P_EMAIL] NVARCHAR(100),
                    [P_FAX] NVARCHAR(50),
                    [P_PIN_CODE] NVARCHAR(15),
                    [P_A_CODE] INT,
                    [P_CUST_TYPE] NVARCHAR(20),
                    [P_COUNTRY_CODE] INT,
                    [P_SM_CODE] INT,
                    [P_CITY_CODE] INT,
                    [P_CITY] NVARCHAR(100),
                    [P_CATEGORY] INT,
                    [P_E_CODE] INT,
                    [ES_DELETE] BIT DEFAULT 0,
                    [P_PAN] NVARCHAR(25),
                    [P_CST] NVARCHAR(50),
                    [P_VAT] NVARCHAR(50),
                    [P_SER_TAX_NO] NVARCHAR(50),
                    [P_ECC_NO] NVARCHAR(50),
                    [P_LBT_NO] NVARCHAR(50),
                    [P_EXC_RANGE] NVARCHAR(50),
                    [P_EXC_DIV] NVARCHAR(50),
                    [P_EXC_COLLECTORATE] NVARCHAR(50),
                    [P_TALLY] NVARCHAR(MAX),
                    [P_CREDITDAYS] INT,
                    [P_TDS] FLOAT,
                    [P_ACTIVE_IND] BIT DEFAULT 1,
                    [P_LBT_IND] BIT DEFAULT 0,
                    FOREIGN KEY ([P_CM_COMP_ID]) REFERENCES [COMPANY_MASTER]([CM_ID])
                )
            END",

            // CUSTPO_MASTER table (Customer PO Master) - matches stored procedure schema
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUSTPO_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[CUSTPO_MASTER] (
                    [CPOM_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [CPOM_P_CODE] INT,
                    [CPOM_PONO] NVARCHAR(100),
                    [CPOM_DOC_NO] INT,
                    [CPOM_TYPE] INT,
                    [CPOM_DATE] DATETIME,
                    [CPOM_CR_DAYS] INT,
                    [CPOM_CM_COMP_ID] INT,
                    [CPOM_WORK_ODR_NO] NVARCHAR(50),
                    [CPOM_PAY_TERM] NVARCHAR(260),
                    [CPOM_AUTH_FLG] BIT DEFAULT 0,
                    [CPOM_PO_DATE] DATETIME,
                    [CPOM_QE_CODE] INT,
                    [CPOM_T_NAME] NVARCHAR(50),
                    [CPOM_T_PER] FLOAT,
                    [CPOM_T_AMT] FLOAT,
                    [CPOM_EXC_PER] FLOAT,
                    [CPOM_EXC_EDU_PER] FLOAT,
                    [CPOM_EXC_HEDU_PER] FLOAT,
                    [CPOM_BASIC_AMT] FLOAT,
                    [CPOM_DISCOUNT_PER] FLOAT,
                    [CPOM_DISCOUNT_AMT] FLOAT,
                    [CPOM_DISCOUNT_REASON] NVARCHAR(50),
                    [CPOM_DEVIATION_AMT] FLOAT,
                    [CPOM_DEVIATION_REASON] NVARCHAR(50),
                    [CPOM_PACKING_AMT] FLOAT,
                    [CPOM_EXC_AMT] FLOAT,
                    [CPOM_ROUNDING] FLOAT,
                    [CPOM_GRAND_TOT] FLOAT,
                    [CPOM_INV_FLAG] INT DEFAULT 0,
                    [CPOM_AM_COUNT] INT DEFAULT 0,
                    [CPOM_FINAL_DEST] NVARCHAR(50),
                    [CPOM_PRE_CARR_BY] NVARCHAR(50),
                    [CPOM_PORT_LOAD] NVARCHAR(50),
                    [CPOM_PORT_DIS] NVARCHAR(50),
                    [CPOM_PLACE_DEL] NVARCHAR(50),
                    [CPOM_BUYER_NAME] NVARCHAR(50),
                    [CPOM_BUYER_ADD] NVARCHAR(150),
                    [CPOM_CURR_CODE] INT,
                    [CPOM_INQ_CODE] INT,
                    [CPOM_IS_VERBAL] BIT DEFAULT 0,
                    [CPOM_PROJECT_CODE] INT,
                    [CPOM_PROJECT_NAME] NVARCHAR(100),
                    [CPOM_AM_DATE] DATETIME,
                    [MODIFY] DATETIME DEFAULT GETDATE(),
                    [ES_DELETE] BIT DEFAULT 0,
                    FOREIGN KEY ([CPOM_P_CODE]) REFERENCES [PARTY_MASTER]([P_CODE])
                )
            END",

            // CUSTPO_DETAIL table (Customer PO Detail) - matches stored procedure schema
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUSTPO_DETAIL]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[CUSTPO_DETAIL] (
                    [CPOD_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [CPOD_CPOM_CODE] INT NOT NULL,
                    [CPOD_I_CODE] INT,
                    [CPOD_UOM_CODE] INT,
                    [CPOD_ORD_QTY] FLOAT,
                    [CPOD_RATE] FLOAT,
                    [CPOD_AMT] FLOAT,
                    [CPOD_DESC] NVARCHAR(MAX),
                    [CPOD_CUST_I_CODE] NVARCHAR(MAX),
                    [CPOD_CUST_I_NAME] NVARCHAR(MAX),
                    [CPOD_STATUS] INT DEFAULT 0,
                    [CPOD_DISPACH] FLOAT DEFAULT 0,
                    [CPOD_IS_ORDER] BIT DEFAULT 0,
                    [CPOD_ST_CODE] INT,
                    [CPOD_CURR_CODE] INT,
                    [CPOD_WO_QTY] FLOAT,
                    [CPOD_MODNO] NVARCHAR(50),
                    [CPOD_MODDATE] DATETIME,
                    [CPOD_AMORTRATE] FLOAT,
                    [CPOD_DIEAMORTRATE] FLOAT,
                    [CPOD_DISC_PER] FLOAT,
                    [CPOD_DISC_AMT] FLOAT,
                    FOREIGN KEY ([CPOD_CPOM_CODE]) REFERENCES [CUSTPO_MASTER]([CPOM_CODE])
                )
            END",

            // ITEM_CATEGORY_MASTER must come before ITEM_MASTER (FK dependency)
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ITEM_CATEGORY_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[ITEM_CATEGORY_MASTER] (
                    [I_CAT_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [I_CAT_NAME] NVARCHAR(50) NOT NULL,
                    [I_CAT_CM_COMP_ID] INT NOT NULL,
                    [I_CAT_SHORTCLOSE] BIT DEFAULT 0,
                    [ES_DELETE] BIT DEFAULT 0,
                    [MODIFY] DATETIME DEFAULT GETDATE(),
                    [ICM_CODE] INT, -- Keep for compatibility
                    [ICM_NAME] NVARCHAR(255),
                    [ICM_COMPANY_ID] INT,
                    [ICM_ACTIVE_IND] BIT DEFAULT 1,
                    [ICM_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [ICM_MODIFIED_DATE] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([I_CAT_CM_COMP_ID]) REFERENCES [COMPANY_MASTER]([CM_ID])
                )
            END",

            // ITEM_MASTER table — after ITEM_CATEGORY_MASTER (FK dependency)
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ITEM_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[ITEM_MASTER] (
                    [I_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [I_CODENO] NVARCHAR(50),
                    [I_NAME] NVARCHAR(255) NOT NULL,
                    [I_DESCRIPTION] NVARCHAR(MAX),
                    [I_CAT_CODE] INT,
                    [ES_DELETE] BIT DEFAULT 0,
                    [I_ACTIVE_IND] BIT DEFAULT 1,
                    [I_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [I_MODIFIED_DATE] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([I_CAT_CODE]) REFERENCES [ITEM_CATEGORY_MASTER]([I_CAT_CODE])
                )
            END",

            // ITEM_UNIT_MASTER table
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ITEM_UNIT_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[ITEM_UNIT_MASTER] (
                    [I_UOM_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [I_UOM_NAME] NVARCHAR(50) NOT NULL,
                    [I_UOM_DESCRIPTION] NVARCHAR(255),
                    [I_UOM_CM_ID] INT NOT NULL,
                    [ES_DELETE] BIT DEFAULT 0,
                    [I_UOM_ACTIVE_IND] BIT DEFAULT 1,
                    [I_UOM_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [I_UOM_MODIFIED_DATE] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([I_UOM_CM_ID]) REFERENCES [COMPANY_MASTER]([CM_ID])
                )
            END",

            // UNIT_MASTER table
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UNIT_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[UNIT_MASTER] (
                    [UM_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [UM_NAME] NVARCHAR(50) NOT NULL,
                    [UM_COMPANY_ID] INT NOT NULL,
                    [UM_ACTIVE_IND] BIT DEFAULT 1,
                    [UM_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [UM_MODIFIED_DATE] DATETIME DEFAULT GETDATE()
                )
            END",

            // CUSTOMER_TYPE_MASTER table - matches stored procedure schema
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUSTOMER_TYPE_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[CUSTOMER_TYPE_MASTER] (
                    [CTM_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [CTM_TYPE_CODE] NVARCHAR(50) NOT NULL,
                    [CTM_TYPE_DESC] NVARCHAR(255),
                    [CTM_DESCRIPTION] NVARCHAR(255),
                    [CTM_FIRST_LETTER] NVARCHAR(1),
                    [CTM_COMPANY_ID] INT NOT NULL,
                    [CTM_CM_COMP_ID] INT NOT NULL,
                    [CTM_FIXED_IND] BIT DEFAULT 0,
                    [CTM_ACTIVE_IND] BIT DEFAULT 1,
                    [ES_DELETE] BIT DEFAULT 0,
                    [MODIFY] BIT DEFAULT 0, -- Changed to BIT to match stored procedure usage
                    [CTM_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [CTM_MODIFIED_DATE] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([CTM_CM_COMP_ID]) REFERENCES [COMPANY_MASTER]([CM_ID])
                )
            END",

            // SO_TYPE_MASTER table - matches stored procedure schema
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SO_TYPE_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[SO_TYPE_MASTER] (
                    [SO_T_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [SO_T_COMP_ID] INT NOT NULL,
                    [SO_T_SHORT_NAME] NVARCHAR(50) NOT NULL,
                    [SO_T_DESC] NVARCHAR(255),
                    [SO_T_FIRST_LETTER] NVARCHAR(1),
                    [ES_DELETE] BIT DEFAULT 0,
                    [MODIFY] BIT DEFAULT 0, -- Changed to BIT to match stored procedure usage
                    [STM_CODE] INT, -- Keep for compatibility
                    [STM_SHORT_NAME] NVARCHAR(50),
                    [STM_DESCRIPTION] NVARCHAR(255),
                    [STM_FIRST_LETTER] NVARCHAR(1),
                    [STM_COMPANY_ID] INT,
                    [STM_FIXED_IND] BIT DEFAULT 0,
                    [STM_ACTIVE_IND] BIT DEFAULT 1,
                    [STM_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [STM_MODIFIED_DATE] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([SO_T_COMP_ID]) REFERENCES [COMPANY_MASTER]([CM_ID])
                )
            END",

            // USER_MASTER table - matches stored procedure schema
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USER_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[USER_MASTER] (
                    [UM_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [UM_USERNAME] NVARCHAR(50) NOT NULL UNIQUE,
                    [UM_PASSWORD] NVARCHAR(255) NOT NULL,
                    [UM_NAME] NVARCHAR(255),
                    [UM_EMAIL] NVARCHAR(255),
                    [UM_CM_ID] INT,
                    [UM_LEVEL] NVARCHAR(50),
                    [IS_ACTIVE] BIT DEFAULT 1,
                    [UM_IS_ADMIN] BIT DEFAULT 0,
                    [UM_LASTLOGIN_DATETIME] DATETIME,
                    [UM_IP_ADDRESS] NVARCHAR(50),
                    [ES_DELETE] BIT DEFAULT 0,
                    FOREIGN KEY ([UM_CM_ID]) REFERENCES [COMPANY_MASTER]([CM_ID])
                )
                
                -- Insert test user (password will be set by EnsureTestUserPasswordAsync)
                IF NOT EXISTS (SELECT * FROM [dbo].[USER_MASTER] WHERE UM_USERNAME = 'TestUser')
                BEGIN
                    INSERT INTO [dbo].[USER_MASTER] ([UM_USERNAME], [UM_PASSWORD], [UM_NAME], [UM_EMAIL], [UM_CM_ID], [IS_ACTIVE], [UM_IS_ADMIN], [ES_DELETE])
                    VALUES ('TestUser', '', 'Test User', 'test@test.com', 1, 1, 1, 0)
                END
            END",

            // USER_RIGHT table — legacy permission bitmask store (read by ERP_GetUserPermissions)
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USER_RIGHT]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[USER_RIGHT] (
                    [UR_CODE]       INT IDENTITY(1,1) PRIMARY KEY,
                    [UR_UM_CODE]    INT NOT NULL,
                    [UR_SM_CODE]    INT NOT NULL,
                    [UR_RIGHTS]     NVARCHAR(7) NOT NULL DEFAULT '0000000',
                    [UR_IS_DELETE]  BIT NOT NULL DEFAULT 0
                )
            END",

            // USER_REFRESH_TOKEN table — JWT refresh token store
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USER_REFRESH_TOKEN]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[USER_REFRESH_TOKEN] (
                    [URT_CODE]        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_USER_REFRESH_TOKEN PRIMARY KEY CLUSTERED,
                    [URT_UM_CODE]     INT           NOT NULL,
                    [URT_TOKEN_HASH]  NVARCHAR(500) NOT NULL,
                    [URT_EXPIRES_AT]  DATETIME2     NOT NULL,
                    [URT_IS_REVOKED]  BIT           NOT NULL CONSTRAINT DF_URT_IS_REVOKED DEFAULT 0,
                    [URT_CREATED_AT]  DATETIME2     NOT NULL CONSTRAINT DF_URT_CREATED_AT DEFAULT GETUTCDATE(),
                    [URT_REPLACED_BY] NVARCHAR(500) NULL,
                    [URT_IP_ADDRESS]  NVARCHAR(50)  NULL,
                    [URT_USER_AGENT]  NVARCHAR(500) NULL
                )
                CREATE NONCLUSTERED INDEX IX_USER_REFRESH_TOKEN_HASH
                    ON [dbo].[USER_REFRESH_TOKEN] ([URT_TOKEN_HASH])
                    WHERE [URT_IS_REVOKED] = 0
                CREATE NONCLUSTERED INDEX IX_USER_REFRESH_TOKEN_UM_CODE
                    ON [dbo].[USER_REFRESH_TOKEN] ([URT_UM_CODE])
            END",

            // ROLES table
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ROLES]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[ROLES] (
                    [RoleId] INT PRIMARY KEY IDENTITY(1,1),
                    [RoleName] NVARCHAR(50) NOT NULL UNIQUE,
                    [IsActive] BIT DEFAULT 1,
                    [CreatedDate] DATETIME DEFAULT GETDATE()
                )
                
                -- Insert default roles
                IF NOT EXISTS (SELECT * FROM [dbo].[ROLES] WHERE RoleName = 'Admin')
                BEGIN
                    INSERT INTO [dbo].[ROLES] ([RoleName], [IsActive]) VALUES ('Admin', 1)
                END
            END",

            // UserRoles table (junction table)
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[UserRoles] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [UserId] INT NOT NULL,
                    [RoleId] INT NOT NULL,
                    [IsActive] BIT DEFAULT 1,
                    [CreatedDate] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([UserId]) REFERENCES [USER_MASTER]([UM_CODE]),
                    FOREIGN KEY ([RoleId]) REFERENCES [ROLES]([RoleId])
                )
                
                -- Assign Admin role to TestUser
                IF NOT EXISTS (SELECT * FROM [dbo].[UserRoles] ur 
                    INNER JOIN [dbo].[USER_MASTER] um ON ur.UserId = um.UM_CODE
                    INNER JOIN [dbo].[ROLES] r ON ur.RoleId = r.RoleId
                    WHERE um.UM_USERNAME = 'TestUser' AND r.RoleName = 'Admin')
                BEGIN
                    INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId], [IsActive])
                    SELECT um.UM_CODE, r.RoleId, 1
                    FROM [dbo].[USER_MASTER] um
                    CROSS JOIN [dbo].[ROLES] r
                    WHERE um.UM_USERNAME = 'TestUser' AND r.RoleName = 'Admin'
                END
            END",

            // TAX_INVOICE_MASTER table
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TAX_INVOICE_MASTER]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[TAX_INVOICE_MASTER] (
                    [TIM_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [TIM_INVOICE_NO] NVARCHAR(50),
                    [TIM_DATE] DATETIME,
                    [TIM_P_CODE] INT,
                    [TIM_CPOM_CODE] INT,
                    [TIM_COMPANY_CODE] INT,
                    [TIM_INVOICE_TYPE] INT,
                    [TIM_NET_AMOUNT] DECIMAL(18,2) DEFAULT 0,
                    [TIM_TAX_AMOUNT] DECIMAL(18,2) DEFAULT 0,
                    [TIM_TOTAL_AMOUNT] DECIMAL(18,2) DEFAULT 0,
                    [TIM_DISCOUNT_PERCENTAGE] DECIMAL(5,2) DEFAULT 0,
                    [TIM_DISCOUNT_AMOUNT] DECIMAL(18,2) DEFAULT 0,
                    [TIM_REMARKS] NVARCHAR(MAX),
                    [TIM_LOCK_IND] BIT DEFAULT 0,
                    [TIM_LOCKED_BY] INT,
                    [TIM_LOCKED_DATE] DATETIME,
                    [TIM_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [TIM_MODIFIED_DATE] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([TIM_P_CODE]) REFERENCES [PARTY_MASTER]([P_CODE]),
                    FOREIGN KEY ([TIM_CPOM_CODE]) REFERENCES [CUSTPO_MASTER]([CPOM_CODE])
                )
            END",

            // TAX_INVOICE_DETAIL table
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TAX_INVOICE_DETAIL]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[TAX_INVOICE_DETAIL] (
                    [TID_CODE] INT PRIMARY KEY IDENTITY(1,1),
                    [TID_TIM_CODE] INT NOT NULL,
                    [TID_I_CODE] INT,
                    [TID_UOM_CODE] INT,
                    [TID_QUANTITY] DECIMAL(18,2),
                    [TID_RATE] DECIMAL(18,2),
                    [TID_AMOUNT] DECIMAL(18,2),
                    [TID_CGST_PERCENTAGE] DECIMAL(5,2) DEFAULT 0,
                    [TID_SGST_PERCENTAGE] DECIMAL(5,2) DEFAULT 0,
                    [TID_IGST_PERCENTAGE] DECIMAL(5,2) DEFAULT 0,
                    [TID_CGST_AMOUNT] DECIMAL(18,2) DEFAULT 0,
                    [TID_SGST_AMOUNT] DECIMAL(18,2) DEFAULT 0,
                    [TID_IGST_AMOUNT] DECIMAL(18,2) DEFAULT 0,
                    [TID_REMARKS] NVARCHAR(MAX),
                    [TID_CREATED_DATE] DATETIME DEFAULT GETDATE(),
                    [TID_MODIFIED_DATE] DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY ([TID_TIM_CODE]) REFERENCES [TAX_INVOICE_MASTER]([TIM_CODE])
                )
            END",

            // Logs table (if not exists)
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[Logs] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [TimeStamp] DATETIME DEFAULT GETDATE(),
                    [Level] NVARCHAR(50),
                    [Message] NVARCHAR(MAX),
                    [Exception] NVARCHAR(MAX),
                    [Properties] NVARCHAR(MAX),
                    [UserId] INT,
                    [RequestId] NVARCHAR(100),
                    [ActionName] NVARCHAR(255)
                )
            END",

            // AUDIT_TRAIL table - matches AuditRepository requirements
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AUDIT_TRAIL]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[AUDIT_TRAIL] (
                    [AUDIT_ID] INT IDENTITY(1,1) PRIMARY KEY,
                    [TABLE_NAME] NVARCHAR(100),
                    [RECORD_ID] INT,
                    [ACTION_TYPE] NVARCHAR(20),
                    [OLD_VALUES] NVARCHAR(MAX),
                    [NEW_VALUES] NVARCHAR(MAX),
                    [CREATED_DATE] DATETIME2 DEFAULT GETDATE(),
                    [CREATED_BY] NVARCHAR(100),
                    [MODIFIED_DATE] DATETIME2,
                    [MODIFIED_BY] NVARCHAR(100),
                    [IP_ADDRESS] NVARCHAR(50),
                    [USER_AGENT] NVARCHAR(500),
                    [SESSION_ID] NVARCHAR(255),
                    [EntityName] NVARCHAR(100),
                    [EntityId] NVARCHAR(50),
                    [Action] NVARCHAR(50),
                    [UserId] NVARCHAR(100),
                    [UserName] NVARCHAR(100),
                    [UserRole] NVARCHAR(50),
                    [CompanyId] NVARCHAR(50),
                    [IpAddress] NVARCHAR(100) NULL DEFAULT '',
                    [UserAgent] NVARCHAR(500),
                    [Endpoint] NVARCHAR(200),
                    [HttpMethod] NVARCHAR(10),
                    [Description] NVARCHAR(1000),
                    [OldValues] NVARCHAR(MAX),
                    [NewValues] NVARCHAR(MAX),
                    [Timestamp] DATETIME DEFAULT GETUTCDATE()
                )
            END",

            // AUDIT_CONFIGURATION table - matches AuditRepository requirements
            @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AUDIT_CONFIGURATION]') AND type in (N'U'))
            BEGIN
                CREATE TABLE [dbo].[AUDIT_CONFIGURATION] (
                    [Id] INT IDENTITY(1,1) PRIMARY KEY,
                    [Endpoint] NVARCHAR(200) NOT NULL,
                    [HttpMethod] NVARCHAR(10) NOT NULL,
                    [EntityName] NVARCHAR(100) NOT NULL,
                    [EntityIdProperty] NVARCHAR(50),
                    [IsEnabled] BIT NOT NULL DEFAULT 1,
                    [TrackPropertyChanges] BIT NOT NULL DEFAULT 1,
                    [TrackOldValues] BIT NOT NULL DEFAULT 1,
                    [TrackNewValues] BIT NOT NULL DEFAULT 1,
                    [Description] NVARCHAR(500),
                    [CreatedDate] DATETIME NOT NULL DEFAULT GETUTCDATE(),
                    [CreatedBy] NVARCHAR(100),
                    CONSTRAINT [UQ_Audit_Endpoint_Method] UNIQUE ([Endpoint], [HttpMethod])
                )
            END"
        };
    }
}

