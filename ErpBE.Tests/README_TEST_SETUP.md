# Integration Test Setup Guide

## Overview
Integration tests use the **production database** with a dedicated test user account. Tests create, verify, and clean up their own test data.

---

## Initial Setup (One-Time)

### Option 1: Automated Setup (Recommended)

Run the PowerShell setup script:

```powershell
.\scripts\Setup-TestUser.ps1
```

This script will:
- ✅ Create a dedicated `TestUser` account
- ✅ Assign **Admin** role for full access
- ✅ Configure credentials for testing

### Option 2: Manual Setup

Run the SQL script in SSMS or using `sqlcmd`:

**File**: `ErpBE.Infrastructure/Scripts/Setup_Test_User.sql`

**Test User Credentials:**
```
Username: TestUser
Password: Test@123
Company ID: 1
Financial Year Code: -2147483641
```

---

## Running Tests

### Option 1: Automated Test Runner (Recommended)

Run the comprehensive PowerShell test automation script:

```powershell
# Run tests only
.\scripts\Run-IntegrationTests.ps1

# Setup TestUser, run tests, and cleanup
.\scripts\Run-IntegrationTests.ps1 -SetupTestUser -CleanupAfterTests

# Run specific tests
.\scripts\Run-IntegrationTests.ps1 -TestFilter "FullyQualifiedName~UnitMaster"

# Run with detailed output
.\scripts\Run-IntegrationTests.ps1 -Verbosity detailed
```

### Option 2: From Visual Studio
1. Open **Test Explorer** (Test → Test Explorer)
2. Click **Run All** or select specific tests
3. View results in the Test Explorer window

### Option 3: From Command Line
```powershell
# Run all tests
dotnet test ErpBE.Tests

# Run specific test class
dotnet test ErpBE.Tests --filter "FullyQualifiedName~UnitMasterControllerTests"

# Run specific test method
dotnet test ErpBE.Tests --filter "FullyQualifiedName~GetUnitMasters_WithValidToken_ShouldReturnOk"

# Run with detailed output
dotnet test ErpBE.Tests --logger "console;verbosity=detailed"
```

---

## Test Data Management

### Automatic Cleanup (Recommended)
Each test should:
1. **Arrange**: Create test data with unique identifiers (e.g., `TEST_UNIT_xyz`)
2. **Act**: Perform the test operation
3. **Assert**: Verify the results
4. **Cleanup**: Delete the test data in a `finally` block or `IDisposable` pattern

### Manual Cleanup

#### Option 1: Automated Cleanup (Recommended)
```powershell
# Cleanup test data only (preserve TestUser)
.\scripts\Cleanup-TestData.ps1

# Cleanup test data AND delete TestUser
.\scripts\Cleanup-TestData.ps1 -DeleteTestUser
```

#### Option 2: SQL Script
Run the cleanup script in SSMS or using `sqlcmd`:

**File**: `ErpBE.Infrastructure/Scripts/Cleanup_Test_Data.sql`

This script will:
- 🧹 Delete all unit masters created by `TestUser`
- 🧹 Delete all records with `TEST_` prefix
- 🧹 Delete associated audit trail entries
- ℹ️ Preserve the `TestUser` account for future tests

---

## Best Practices

### ✅ DO:
- Use unique test data identifiers (e.g., `TEST_UNIT_{Guid}`)
- Clean up test data in `finally` blocks
- Use `TestUser` credentials for authentication
- Verify test data is deleted after tests complete

### ❌ DON'T:
- Use production user accounts for testing
- Leave test data in the database
- Create duplicate test data without cleanup
- Modify existing production data

---

## Test Data Naming Conventions

To ensure easy identification and cleanup:

| Entity | Naming Pattern | Example |
|--------|---------------|---------|
| Unit Master | `TEST_UNIT_{description}` | `TEST_UNIT_KG` |
| Test descriptions | Include "Test" or "Testing" | `"Test Unit for Integration"` |

---

## Troubleshooting

### Problem: Login fails with 401 Unauthorized
**Solution**: Verify `TestUser` exists by running:
```sql
SELECT * FROM USER_MASTER WHERE UM_USERNAME = 'TestUser';
```
If not found, run `Setup_Test_User.sql`

### Problem: Test fails with 403 Forbidden
**Solution**: Verify `TestUser` has Admin role:
```sql
SELECT UM.UM_USERNAME, R.ROLE_NAME 
FROM USER_MASTER UM
INNER JOIN UserRoles UR ON UM.UM_CODE = UR.USER_ID
INNER JOIN ROLES R ON UR.ROLE_ID = R.ROLE_ID
WHERE UM.UM_USERNAME = 'TestUser';
```
If Admin role is missing, re-run `Setup_Test_User.sql`

### Problem: Test data is not being cleaned up
**Solution**: 
1. Check for exceptions in test execution
2. Ensure cleanup code is in `finally` block
3. Manually run `Cleanup_Test_Data.sql`

---

## Database Connection

Tests use the connection string from `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SANTOSH\\SQLEXPRESS;Database=sunelectronics_erp_db;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=false"
}
```

Ensure SQL Server is running and accessible before running tests.

---

## Running Tests in CI/CD

For automated builds:
1. Ensure test database is accessible
2. Run `Setup_Test_User.sql` as part of build setup
3. Run tests: `dotnet test`
4. Run `Cleanup_Test_Data.sql` as part of teardown
5. Generate test reports

---

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Verify test user setup
3. Check SQL Server connectivity
4. Review test logs for detailed error messages

