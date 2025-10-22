# Database Change Workflow

## Overview
This document outlines the complete workflow for managing database changes from development to production and keeping test environments in sync.

## Workflow Steps

### 1. Development Phase

#### Step 1: Create New Stored Procedure
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

#### Step 2: Add to Test Scripts
```sql
-- test-scripts/02-create-procedures.sql
-- Add the new procedure here
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

#### Step 3: Create Application Code
```csharp
// ErpBE.Application/UserManagement/Queries/GetUserByEmailQuery.cs
public class GetUserByEmailQuery : IRequest<UserDto?>
{
    public string Email { get; set; } = string.Empty;
}

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUserManagementRepository _repository;
    
    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetUserByEmailAsync(request.Email);
    }
}
```

#### Step 4: Add Repository Method
```csharp
// ErpBE.Infrastructure/Repositories/UserManagementRepository.cs
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

#### Step 5: Create Controller Endpoint
```csharp
// ErpBE.API/Controllers/Admin/UserController.cs
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

#### Step 6: Test Locally
```powershell
# Reset test database
.\scripts\reset-test-db.ps1

# Run tests
dotnet test ErpBE.Tests

# Test manually
dotnet run --project ErpBE.API
# Test the new endpoint
```

#### Step 7: Commit to Repository
```bash
git add .
git commit -m "Add GetUserByEmail stored procedure and endpoint"
git push origin feature/user-by-email
```

### 2. CI/CD Pipeline

#### Step 1: Pull Request
- Create PR with database changes
- CI runs tests against test container
- Reviewers approve changes

#### Step 2: Merge to Main
- PR merged to main branch
- CI/CD pipeline triggers

#### Step 3: Production Deployment
```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Deploy to Production
      run: |
        # Deploy application
        # Run database migration scripts
        # Update production database
```

#### Step 4: Update Test Container
```yaml
# .github/workflows/update-test-schema.yml
name: Update Test Schema

on:
  push:
    branches: [main]

jobs:
  update-test:
    runs-on: ubuntu-latest
    steps:
    - name: Update Test Database
      run: |
        # Sync schema from production
        ./scripts/sync-production-schema.ps1
        
        # Update database project
        ./scripts/update-database-project.ps1
        
        # Reset test container
        ./scripts/reset-test-db.ps1
```

### 3. Production Database Update

#### Option A: Manual Deployment
```sql
-- Run on production database
EXEC SP_GetUserByEmail 'test@example.com'
```

#### Option B: Automated Deployment
```powershell
# Deploy database changes
.\scripts\deploy-to-production.ps1
```

### 4. Test Container Sync

#### Automatic Sync (Recommended)
```powershell
# This runs automatically after production deployment
.\scripts\sync-production-schema.ps1
.\scripts\reset-test-db.ps1
```

#### Manual Sync
```powershell
# If you need to sync manually
.\scripts\update-database-project.ps1
.\scripts\reset-test-db.ps1
dotnet test ErpBE.Tests
```

## Benefits of This Workflow

✅ **Version Control** - All database changes tracked in Git  
✅ **Testing** - Changes tested before production  
✅ **Rollback** - Can revert changes if issues arise  
✅ **Consistency** - Test environment always matches production  
✅ **Automation** - Minimal manual intervention required  
✅ **Audit Trail** - Complete history of all changes  

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

## File Structure

```
ErpBE.Database/
├── Tables/
│   ├── USER_MASTER.sql
│   └── ...
├── StoredProcedures/
│   ├── SP_GetUserByEmail.sql  # New procedure
│   └── ...
└── Functions/
    └── ...

test-scripts/
├── 01-init-database.sql
├── 02-create-procedures.sql   # Updated with new procedure
└── 03-seed-test-data.sql

scripts/
├── start-test-db.ps1
├── stop-test-db.ps1
├── reset-test-db.ps1
├── sync-production-schema.ps1
├── update-database-project.ps1
└── deploy-to-production.ps1
```
