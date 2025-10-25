# 🔧 Common Issues and Fixes

Quick reference for common development issues and their solutions.

---

## 🔒 Issue: DLL Locked by Running Process

### Symptoms:
```
Could not copy "ErpBE.Infrastructure.dll" to "bin\Debug\net9.0\ErpBE.Infrastructure.dll". 
Exceeded retry count of 10. Failed. 
The file is locked by: "ErpBE.API (24312)"
```

### Cause:
The API is still running (either in background or in a terminal), locking the DLL files.

### Fix:

**Option 1: Stop All Dotnet Processes (PowerShell)**
```powershell
Get-Process -Name "dotnet" | Stop-Process -Force
```

**Option 2: Stop from Task Manager**
1. Press `Ctrl + Shift + Esc`
2. Find all `dotnet.exe` processes
3. Right-click → End Task

**Option 3: Stop from Command Line**
```cmd
taskkill /F /IM dotnet.exe
```

### Prevention:
- Always stop the API (Ctrl+C) when done testing
- Close terminals running the API
- Use `dotnet run` instead of background processes for development

---

## 🚫 Issue: Port Already in Use

### Symptoms:
```
Failed to bind to address https://localhost:7032: address already in use
```

### Cause:
Another instance of the API is already running on port 7032.

### Fix:

**Find and Kill Process on Port 7032 (PowerShell)**
```powershell
$port = 7032
$process = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess
if ($process) {
    Stop-Process -Id $process -Force
    Write-Host "Process on port $port stopped"
} else {
    Write-Host "No process found on port $port"
}
```

**Alternative (CMD)**
```cmd
netstat -ano | findstr :7032
taskkill /F /PID <PID_FROM_ABOVE>
```

---

## 🗃️ Issue: Database Connection Failed

### Symptoms:
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server
```

### Cause:
- SQL Server is not running
- Connection string is incorrect
- Firewall blocking connection

### Fix:

1. **Check SQL Server Status:**
   - Open SQL Server Configuration Manager
   - Ensure SQL Server service is running

2. **Verify Connection String:**
   - Check `appsettings.json` → `ConnectionStrings:DefaultConnection`
   - Ensure server name, database name, credentials are correct

3. **Test Connection:**
   ```sql
   -- In SSMS, try to connect with the same credentials
   ```

---

## 🔑 Issue: 401 Unauthorized on API Calls

### Symptoms:
API returns `401 Unauthorized` status

### Cause:
- JWT token expired
- Token not included in request
- Invalid token

### Fix:

1. **Login again to get fresh token:**
   ```http
   POST https://localhost:7032/api/Auth/login
   {
     "username": "Mohan",
     "password": "1234",
     "companyId": 1,
     "financialYearCode": -2147483641
   }
   ```

2. **Include token in request:**
   ```
   Authorization: Bearer YOUR_TOKEN_HERE
   ```

3. **Check token expiry:**
   - Tokens expire after 24 hours (default)
   - Check `appsettings.json` → `JwtSettings:ExpiryInHours`

---

## 🚨 Issue: 403 Forbidden on API Calls

### Symptoms:
API returns `403 Forbidden` status

### Cause:
User doesn't have required role/permission for the endpoint

### Fix:

1. **Check user roles:**
   ```sql
   SELECT U.UM_USER_NAME, R.RM_NAME
   FROM USER_MASTER U
   INNER JOIN UserRoles UR ON UR.UserId = U.UM_CODE
   INNER JOIN Roles R ON R.Id = UR.RoleId
   WHERE U.UM_USER_NAME = 'YourUsername'
   ```

2. **Add required role:**
   ```sql
   -- Example: Add Admin role to user
   DECLARE @UserId INT = (SELECT UM_CODE FROM USER_MASTER WHERE UM_USER_NAME = 'Mohan')
   DECLARE @RoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin')
   
   IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
   BEGIN
       INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)
   END
   ```

---

## 📦 Issue: NuGet Package Restore Failed

### Symptoms:
```
error NU1101: Unable to find package 'PackageName'
```

### Cause:
- Package source not configured
- Network issue
- Package version doesn't exist

### Fix:

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Clear NuGet cache:**
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

3. **Check NuGet sources:**
   ```bash
   dotnet nuget list source
   ```

---

## 🔨 Issue: Build Failed with "CS0246: Type or namespace not found"

### Symptoms:
```
error CS0246: The type or namespace name 'SomeClass' could not be found
```

### Cause:
- Missing using statement
- Missing project reference
- Missing NuGet package

### Fix:

1. **Add using statement:**
   ```csharp
   using YourNamespace;
   ```

2. **Add project reference:**
   ```bash
   dotnet add reference path/to/Project.csproj
   ```

3. **Add NuGet package:**
   ```bash
   dotnet add package PackageName
   ```

4. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

---

## 🧪 Issue: Tests Failing with Database Errors

### Symptoms:
Tests fail with "Invalid object name" or "Cannot open database"

### Cause:
- Test database not set up
- Connection string pointing to wrong database

### Fix:

1. **Check test connection string:**
   - Tests should use production database (as per current setup)
   - Ensure connection string in test project is correct

2. **Verify stored procedures exist:**
   ```sql
   SELECT * FROM sys.procedures WHERE name LIKE 'ERP_%'
   ```

3. **Run database scripts:**
   ```bash
   # Deploy missing stored procedures
   ```

---

## 📝 Issue: Logs Not Appearing

### Symptoms:
- Logs table is empty
- No logs in Seq/file

### Cause:
- Logging not configured
- Log level too high (only errors logged)
- Connection string incorrect

### Fix:

1. **Check log level:**
   - `appsettings.json` → `Serilog:MinimumLevel:Default`
   - Should be `Debug` or `Information` for development

2. **Verify Logs table exists:**
   ```sql
   SELECT TOP 10 * FROM Logs ORDER BY Id DESC
   ```

3. **Check connection string:**
   - `appsettings.json` → `ConnectionStrings:DefaultConnection`

---

## 🌐 Issue: CORS Error in Browser

### Symptoms:
```
Access to XMLHttpRequest has been blocked by CORS policy
```

### Cause:
Frontend origin not allowed in API CORS policy

### Fix:

1. **Add origin to CORS policy in `Program.cs`:**
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowAll", builder =>
       {
           builder.WithOrigins("http://localhost:3000", "http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
       });
   });
   ```

2. **Use the policy:**
   ```csharp
   app.UseCors("AllowAll");
   ```

---

## 🔄 Issue: Changes Not Reflected After Build

### Symptoms:
Code changes don't appear to be working after rebuild

### Cause:
- Old DLLs cached
- Browser cache (for frontend)
- IIS/Kestrel not restarted

### Fix:

1. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

2. **Clear browser cache:**
   - Press `Ctrl + Shift + Delete`
   - Or use incognito mode

3. **Restart API:**
   - Stop and start the API
   - Or use `dotnet watch run` for auto-reload

---

## 📊 Quick Commands Reference

### Stop All API Processes:
```powershell
Get-Process -Name "dotnet" | Stop-Process -Force
```

### Clean Build:
```bash
dotnet clean
dotnet build
```

### Run API:
```bash
dotnet run --project ErpBE.API/ErpBE.API.csproj
```

### Run Tests:
```bash
dotnet test
```

### Check for Running Processes:
```powershell
Get-Process -Name "dotnet"
```

### Check Port Usage:
```powershell
Get-NetTCPConnection -LocalPort 7032
```

---

## 🆘 Still Having Issues?

1. **Check logs:**
   - SQL Server: `SELECT TOP 100 * FROM Logs ORDER BY Id DESC`
   - Console output from the API

2. **Restart everything:**
   - Stop all dotnet processes
   - Close Visual Studio
   - Restart SQL Server
   - Reopen Visual Studio

3. **Last resort:**
   ```bash
   # Clean everything
   dotnet clean
   # Delete bin and obj folders
   Get-ChildItem -Path . -Include bin,obj -Recurse | Remove-Item -Recurse -Force
   # Restore and rebuild
   dotnet restore
   dotnet build
   ```

---

**Last Updated:** October 24, 2025


