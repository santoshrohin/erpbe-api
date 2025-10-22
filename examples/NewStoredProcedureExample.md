# Example: Adding a New Stored Procedure

This example shows the complete workflow for adding a new stored procedure from development to production.

## Scenario
We want to add a stored procedure `SP_GetUserByEmail` to get user details by email address.

## Step 1: Create the Stored Procedure

### 1.1 Add to Database Project
```sql
-- ErpBE.Database/StoredProcedures/SP_GetUserByEmail.sql
CREATE PROCEDURE [dbo].[SP_GetUserByEmail]
    @Email VARCHAR(100)
AS
BEGIN
    SET NOCOUNT OFF;
    
    SELECT 
        UM_CODE AS UserId,
        UM_USERNAME AS Username,
        UM_NAME AS Name,
        UM_EMAIL AS Email,
        UM_CM_ID AS CompanyId,
        -2147483641 AS FinancialYearCode,
        CASE WHEN IS_ACTIVE = 1 THEN 1 ELSE 0 END AS IsActive,
        CASE WHEN UM_IS_ADMIN = 1 THEN 1 ELSE 0 END AS IsAdmin,
        UM_LASTLOGIN_DATETIME AS LastLoginDateTime,
        UM_IP_ADDRESS AS IpAddress
    FROM USER_MASTER
    WHERE UM_EMAIL = @Email
      AND (ES_DELETE = 0 OR ES_DELETE IS NULL);
END
```

### 1.2 Add to Test Scripts
```sql
-- test-scripts/02-create-procedures.sql
-- Add this at the end of the file

-- SP_GetUserByEmail
CREATE PROCEDURE [dbo].[SP_GetUserByEmail]
    @Email VARCHAR(100)
AS
BEGIN
    SET NOCOUNT OFF;
    
    SELECT 
        UM_CODE AS UserId,
        UM_USERNAME AS Username,
        UM_NAME AS Name,
        UM_EMAIL AS Email,
        UM_CM_ID AS CompanyId,
        -2147483641 AS FinancialYearCode,
        CASE WHEN IS_ACTIVE = 1 THEN 1 ELSE 0 END AS IsActive,
        CASE WHEN UM_IS_ADMIN = 1 THEN 1 ELSE 0 END AS IsAdmin,
        UM_LASTLOGIN_DATETIME AS LastLoginDateTime,
        UM_IP_ADDRESS AS IpAddress
    FROM USER_MASTER
    WHERE UM_EMAIL = @Email
      AND (ES_DELETE = 0 OR ES_DELETE IS NULL);
END
GO
```

## Step 2: Create Application Code

### 2.1 Add Repository Interface
```csharp
// ErpBE.Domain/Interfaces/IUserManagementRepository.cs
// Add this method to the existing interface
Task<UserDto?> GetUserByEmailAsync(string email);
```

### 2.2 Implement Repository Method
```csharp
// ErpBE.Infrastructure/Repositories/UserManagementRepository.cs
// Add this method to the existing class
public async Task<UserDto?> GetUserByEmailAsync(string email)
{
    using var connection = new SqlConnection(_connectionString);
    
    var parameters = new DynamicParameters();
    parameters.Add("@Email", email);
    
    return await connection.QueryFirstOrDefaultAsync<UserDto>(
        "SP_GetUserByEmail", 
        parameters, 
        commandType: CommandType.StoredProcedure);
}
```

### 2.3 Create Query and Handler
```csharp
// ErpBE.Application/UserManagement/Queries/GetUserByEmailQuery.cs
using MediatR;
using ErpBE.Domain.DTOs;

namespace ErpBE.Application.UserManagement.Queries
{
    public class GetUserByEmailQuery : IRequest<UserDto?>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
    {
        private readonly IUserManagementRepository _repository;
        
        public GetUserByEmailQueryHandler(IUserManagementRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetUserByEmailAsync(request.Email);
        }
    }
}
```

### 2.4 Add Controller Endpoint
```csharp
// ErpBE.API/Controllers/Admin/UserController.cs
// Add this method to the existing controller
[HttpGet("by-email/{email}")]
[Authorize(Roles = "Admin,SalesManager")]
public async Task<IActionResult> GetUserByEmail(string email)
{
    var query = new GetUserByEmailQuery { Email = email };
    var user = await _mediator.Send(query);
    
    if (user == null)
        return NotFound();
        
    return Ok(user);
}
```

## Step 3: Test Locally

### 3.1 Reset Test Database
```powershell
.\scripts\reset-test-db.ps1
```

### 3.2 Run Tests
```powershell
dotnet test ErpBE.Tests
```

### 3.3 Test Manually
```powershell
# Start API
dotnet run --project ErpBE.API

# Test the endpoint
curl -H "Authorization: Bearer YOUR_TOKEN" \
     "https://localhost:7032/api/User/by-email/test@example.com"
```

## Step 4: Deploy to Production

### 4.1 Dry Run (Test deployment)
```powershell
.\scripts\deploy-to-production.ps1 -DryRun
```

### 4.2 Deploy to Production
```powershell
.\scripts\deploy-to-production.ps1
```

### 4.3 Verify in Production
```sql
-- Test the procedure in production
EXEC SP_GetUserByEmail 'test@example.com'
```

## Step 5: Sync Test Environment

### 5.1 Sync Schema
```powershell
.\scripts\sync-production-schema.ps1
```

### 5.2 Reset Test Database
```powershell
.\scripts\reset-test-db.ps1
```

### 5.3 Run Tests
```powershell
dotnet test ErpBE.Tests
```

## Step 6: Commit to Repository

```bash
git add .
git commit -m "Add GetUserByEmail stored procedure and endpoint"
git push origin feature/user-by-email
```

## Complete Workflow Summary

1. **Development** → Create SP in repository
2. **Testing** → Test locally with container
3. **Deployment** → Deploy to production
4. **Sync** → Sync test environment
5. **Validation** → Run tests to ensure everything works
6. **Version Control** → Commit changes to repository

## Benefits

✅ **Version Controlled** - All changes tracked in Git  
✅ **Tested** - Changes tested before production  
✅ **Automated** - Minimal manual intervention  
✅ **Consistent** - Test environment matches production  
✅ **Rollback** - Can revert changes if needed  
✅ **Audit Trail** - Complete history of changes  

## Quick Commands

```powershell
# Start development
.\scripts\start-test-db.ps1

# Test changes
dotnet test ErpBE.Tests

# Deploy to production
.\scripts\deploy-to-production.ps1

# Sync test environment
.\scripts\sync-production-schema.ps1
.\scripts\reset-test-db.ps1
```
