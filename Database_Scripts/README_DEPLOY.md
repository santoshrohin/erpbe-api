# Database Deployment Guide

## 📋 New Stored Procedures to Deploy

After the CQRS refactoring, we created 2 new stored procedures that need to be deployed to your database:

1. **SP_GetLogs** - Retrieves logs with filtering, pagination, and searching
2. **SP_GetUserRolesByUserId** - Gets user roles by user ID

---

## 🚀 Deployment Steps

### Option 1: Using SQL Server Management Studio (SSMS)

1. Open **SQL Server Management Studio**
2. Connect to your database server
3. Open the file: `Database_Scripts/Deploy_New_StoredProcedures.sql`
4. **IMPORTANT**: Edit line 7 and replace `[YOUR_DATABASE_NAME]` with your actual database name
5. Execute the script (F5 or click Execute)
6. Verify the output messages show success

### Option 2: Using sqlcmd (Command Line)

```powershell
# Replace the values below with your actual connection details
sqlcmd -S YOUR_SERVER_NAME -d YOUR_DATABASE_NAME -i "Database_Scripts\Deploy_New_StoredProcedures.sql"
```

### Option 3: Using Azure Data Studio

1. Open **Azure Data Studio**
2. Connect to your database
3. Open the file: `Database_Scripts/Deploy_New_StoredProcedures.sql`
4. **IMPORTANT**: Edit line 7 and replace `[YOUR_DATABASE_NAME]` with your actual database name
5. Click **Run** or press F5

---

## ✅ Verify Deployment

After deploying, verify the stored procedures exist:

```sql
-- Check if SP_GetLogs exists
SELECT * FROM sys.procedures WHERE name = 'SP_GetLogs';

-- Check if SP_GetUserRolesByUserId exists
SELECT * FROM sys.procedures WHERE name = 'SP_GetUserRolesByUserId';

-- Test SP_GetLogs
DECLARE @TotalCount INT;
EXEC SP_GetLogs 
    @PageNumber = 1, 
    @PageSize = 10, 
    @TotalCount = @TotalCount OUTPUT;
SELECT @TotalCount AS TotalLogs;
```

---

## 🔧 After Deployment

Once you've deployed the stored procedures, run the tests again:

```powershell
dotnet test --filter "FullyQualifiedName~LogsController"
```

All 13 Logs Controller tests should now pass!

---

## 📝 What These Stored Procedures Do

### SP_GetLogs
- **Purpose**: Retrieve logs with advanced filtering
- **Parameters**:
  - `@PageNumber` - Page number for pagination
  - `@PageSize` - Number of records per page
  - `@Level` - Filter by log level (Information, Warning, Error, etc.)
  - `@SearchTerm` - Search in log messages
  - `@StartDate` - Filter logs from this date
  - `@EndDate` - Filter logs until this date
  - `@TotalCount` - OUTPUT parameter with total record count

### SP_GetUserRolesByUserId
- **Purpose**: Get all roles assigned to a specific user
- **Parameters**:
  - `@UserId` - The user ID (UM_CODE from USER_MASTER)
- **Returns**: List of role names for the user

---

## 🐛 Troubleshooting

### Error: "Cannot find the object 'Logs'"
- The `Logs` table doesn't exist. Check if Serilog created it.
- Run: `SELECT * FROM sys.tables WHERE name = 'Logs'`

### Error: "Cannot find stored procedure 'SP_GetUserRoles'"
- `SP_GetUserRoles` is a dependency for `SP_GetUserRolesByUserId`
- Ensure it exists in your database (it should already be there from earlier work)

---

## 📞 Need Help?

If you encounter any errors during deployment, check:
1. Database connection is working
2. You have CREATE PROCEDURE permissions
3. The database name is correct
4. All dependent tables exist (Logs, USER_MASTER, Roles, UserRoles)

