# Test Database Safety Guide

## 🚨 IMPORTANT: Production Database Protection

**Tests will NEVER modify your production database** - multiple safety mechanisms are in place:

## Safety Mechanisms

### 1. **Production Database Blocking**
Tests will **FAIL IMMEDIATELY** if configured to use the production database (`db_a2ea4b_sunv2` without "Test" in the name).

### 2. **Automatic Database Cleanup**
After each test, Respawner automatically cleans up all test data. This ensures:
- No test data persists between test runs
- Database returns to a clean state after each test
- Tests are isolated and don't interfere with each other

### 3. **Test Database Configuration**
Tests use a separate test database configured in `ErpBE.Tests/appsettings.json`:
- Database name MUST contain "Test" or "_Test" (e.g., `db_a2ea4b_sunv2_Test`)
- This is checked at test startup and tests will fail if not configured correctly

### 4. **Testcontainers Option (Optional)**
For complete isolation, you can use Testcontainers (Docker-based isolated databases):
- Set environment variable: `USE_TESTCONTAINERS=true`
- This creates a completely isolated database in a Docker container
- No risk of affecting any existing database

## Configuration Options

### Option 1: Separate Test Database (Recommended - No Docker Required)
Configure in `ErpBE.Tests/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB_Test;User Id=...;Password=...;"
  }
}
```

### Option 2: Testcontainers (Complete Isolation - Requires Docker)
Set environment variable:
```bash
$env:USE_TESTCONTAINERS="true"
dotnet test
```

## What Happens During Tests

1. **Before Tests**: Respawner initializes and creates a restore point
2. **During Tests**: Tests can create/read/update/delete data safely
3. **After Each Test**: Respawner automatically cleans up all test data
4. **Safety Check**: Production database usage is blocked with clear error messages

## Verification

You can verify the test database is configured correctly by:
1. Checking `ErpBE.Tests/appsettings.json` - database name should contain "Test"
2. Running tests - they will fail immediately if production database is detected
3. Checking test output - safety checks will be logged

## Troubleshooting

**Error: "SAFETY CHECK FAILED: Tests are configured to use PRODUCTION database!"**
- Solution: Update `ErpBE.Tests/appsettings.json` to use a test database (name must contain "Test")

**Error: "CRITICAL: Could not initialize Respawner"**
- Solution: Ensure the test database exists and connection string is correct
- Respawner is required for safe test execution

**Error: "CRITICAL: Could not reset database after test"**
- Solution: Check database permissions - Respawner needs DELETE permissions
- This error indicates test data may not be cleaned up

## Summary

✅ **Tests use separate test database** (not production)
✅ **Automatic cleanup** after each test
✅ **Production database protection** (tests fail if production DB detected)
✅ **Respawner** ensures clean state between tests
✅ **Testcontainers** option for complete isolation

**Your production database is SAFE!** 🛡️

