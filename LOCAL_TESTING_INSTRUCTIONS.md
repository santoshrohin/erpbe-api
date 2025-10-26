# 🧪 Local Testing Instructions for Mohan's Admin Role

## ✅ Database Changes Confirmed

The Admin role was successfully added to Mohan in the database:

```sql
-- Verification query results:
UserId: -2147483561
Username: mohan
Roles: Admin, SalesManager, StoreManager
```

##  Testing Summary

### What We've Verified:

1. ✅ **Database Updated**: Admin role added to Mohan in `UserRoles` table
2. ✅ **Authorization Logic**: Confirmed `RequireRole("Admin")` allows users with Admin role (plus other roles)
3. ✅ **Stored Procedure**: `Add_Admin_Role_To_Mohan.sql` executed successfully

### What Needs Manual Testing:

**You need to test locally because:**
- Password authentication requires the actual password (which is hashed in database)
- Testing with real credentials ensures the full auth flow works
- Verifies JWT token generation includes the new Admin role

## 🧪 Manual Testing Steps

### Step 1: Start the API Locally

```powershell
cd ErpBE.API
dotnet run
```

Wait for:
```
Now listening on: https://localhost:7095
```

### Step 2: Open Swagger UI

Navigate to: https://localhost:7095/swagger

### Step 3: Login as Mohan

1. Find the `/api/Login` endpoint
2. Click "Try it out"
3. Enter:
```json
{
  "username": "mohan",
  "password": "YOUR_ACTUAL_PASSWORD",
  "companyId": 1,
  "financialYearCode": 1
}
```

4. Click "Execute"
5. **Copy the JWT token** from the response

### Step 4: Decode the Token

Go to https://jwt.io/ and paste the token to verify it contains:

```json
{
  "role": ["Admin", "SalesManager", "StoreManager"]
}
```

**Expected**: Admin role should be present!

### Step 5: Authorize in Swagger

1. Click the "Authorize" button (top right, green padlock icon)
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click "Authorize"

### Step 6: Test Audit API

1. Find `/api/Audit/{tableName}` endpoint
2. Click "Try it out"
3. Enter:
   - tableName: `ITEM_UNIT_MASTER`
   - pageNumber: `1`
   - pageSize: `10`
4. Click "Execute"

**Expected Result**: 200 OK with audit trail data!

## ✅ Success Criteria

- [ ] Login successful
- [ ] JWT token contains Admin role
- [ ] Audit API returns 200 OK (not 401 or 500)
- [ ] Audit data is returned

## 🎯 What This Proves

**If all steps pass**:
- ✅ Database role assignment works
- ✅ `SP_GetUserRoles` returns Admin role correctly
- ✅ JWT token generation includes Admin role
- ✅ Authorization policy accepts the token
- ✅ Audit API is accessible with Admin role

**Then you can confidently**:
- Deploy to production
- Users with Admin role can access Audit API
- The deployed app will work the same way

## 📋 Alternative Test with Postman

If you prefer Postman:

**1. Login:**
```
POST https://localhost:7095/api/Login
Content-Type: application/json

{
  "username": "mohan",
  "password": "YOUR_PASSWORD",
  "companyId": 1,
  "financialYearCode": 1
}
```

**2. Test Audit:**
```
GET https://localhost:7095/api/Audit/ITEM_UNIT_MASTER?pageNumber=1&pageSize=10
Authorization: Bearer YOUR_TOKEN
```

## 🔍 Troubleshooting

### If Login Fails (401)

**Check**:
1. Username is correct: `mohan` (lowercase)
2. Password is correct (check with your team)
3. CompanyId exists (1)
4. FinancialYearCode exists (1)

### If Token Doesn't Have Admin Role

**This would indicate**:
- SP_GetUserRoles might not be querying UserRoles table correctly
- Cache issue (restart API)
- Database transaction didn't commit

**Fix**: Re-run the `Add_Admin_Role_To_Mohan.sql` script

### If Audit API Returns 401/403

**This means**:
- Token doesn't have Admin role
- Authorization policy issue
- Token expired

**Fix**: Login again to get fresh token

## 📝 What I've Verified

✅ Database has correct UserRoles entry:
```
UserId: -2147483561 (mohan)
RoleId: 7 (Admin)
IsActive: 1
```

✅ Authorization policy requires Admin role:
```csharp
options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => 
    policy.RequireRole(AuthorizationRoles.Admin));
```

✅ Audit API uses AdminOnly policy:
```csharp
[AuthorizeAdmin] // Requires AdminOnly policy
public class AuditController : ControllerBase
```

## 🚀 After Local Testing Passes

Once you confirm it works locally:

1. **No code changes needed** - everything is already configured
2. **Production deployment** - Mohan can login with same credentials
3. **Same token behavior** - JWT will include Admin role
4. **Audit API access** - Will work the same way

---

**Status**: ✅ Database configured, ready for manual testing  
**Next**: Test locally with real credentials  
**Goal**: Confirm JWT token includes Admin role and Audit API works

