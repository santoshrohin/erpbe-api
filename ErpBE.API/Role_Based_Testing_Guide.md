# Role-Based Authorization Testing Guide

## Overview
This guide helps you test the role-based authorization system with the created test users.

## Test Users Created

| Username | Password | Role | Access Level |
|----------|----------|------|--------------|
| `ERPADMIN` | `admin` | Admin | Full access to all modules |
| `SALESMGR` | `admin` | SalesManager | Sales module + read access |
| `STOREMGR` | `admin` | StoreManager | Store module + read access |
| `PURCHASEMGR` | `admin` | PurchaseManager | Purchase module + read access |
| `READONLYMGR` | `admin` | ReadOnlyManager | Read-only access to all modules |
| `UTILITYMGR` | `admin` | UtilityManager | Utility module + limited read access |

## API Testing Steps

### 1. Test Login with Different Users

#### Admin User Login
```bash
curl -X POST "https://localhost:5001/api/Login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "ERPADMIN",
    "password": "admin",
    "companyId": 1,
    "financialYearCode": 2024
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "ERPADMIN",
  "companyId": 1,
  "companyCode": "001",
  "companyName": "Your Company",
  "email": "admin@company.com",
  "roles": ["Admin"],
  "permissions": []
}
```

#### Sales Manager Login
```bash
curl -X POST "https://localhost:5001/api/Login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "SALESMGR",
    "password": "admin",
    "companyId": 1,
    "financialYearCode": 2024
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "SALESMGR",
  "companyId": 1,
  "companyCode": "001",
  "companyName": "Your Company",
  "email": "sales@company.com",
  "roles": ["SalesManager"],
  "permissions": []
}
```

### 2. Test API Endpoints with Different Roles

#### Test Farmer Management

##### GET /api/Farmer (Should work for all roles)
```bash
# Test with Admin token
curl -X GET "https://localhost:5001/api/Farmer" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"

# Test with SalesManager token
curl -X GET "https://localhost:5001/api/Farmer" \
  -H "Authorization: Bearer YOUR_SALES_TOKEN"

# Test with ReadOnlyManager token
curl -X GET "https://localhost:5001/api/Farmer" \
  -H "Authorization: Bearer YOUR_READONLY_TOKEN"
```

**Expected:** All should return 200 OK with farmer data.

##### POST /api/Farmer (Should work for Admin, SalesManager, StoreManager, PurchaseManager)
```bash
# Test with Admin token (should work)
curl -X POST "https://localhost:5001/api/Farmer" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "farmerName": "Test Farmer",
    "farmerCode": "TF001",
    "farmerAddress": "Test Address",
    "branchId": 1,
    "lineId": 1
  }'

# Test with ReadOnlyManager token (should fail)
curl -X POST "https://localhost:5001/api/Farmer" \
  -H "Authorization: Bearer YOUR_READONLY_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "farmerName": "Test Farmer",
    "farmerCode": "TF001",
    "farmerAddress": "Test Address",
    "branchId": 1,
    "lineId": 1
  }'
```

**Expected:** Admin should get 201 Created, ReadOnlyManager should get 403 Forbidden.

#### Test Branch Management

##### GET /api/Branch (Should work for all roles)
```bash
curl -X GET "https://localhost:5001/api/Branch" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected:** All roles should return 200 OK.

##### POST /api/Branch (Should work only for Admin)
```bash
# Test with Admin token (should work)
curl -X POST "https://localhost:5001/api/Branch" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "branchName": "Test Branch"
  }'

# Test with SalesManager token (should fail)
curl -X POST "https://localhost:5001/api/Branch" \
  -H "Authorization: Bearer YOUR_SALES_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "branchName": "Test Branch"
  }'
```

**Expected:** Admin should get 201 Created, SalesManager should get 403 Forbidden.

#### Test Placement Management

##### POST /api/Placement (Should work for Admin and SalesManager)
```bash
# Test with SalesManager token (should work)
curl -X POST "https://localhost:5001/api/Placement" \
  -H "Authorization: Bearer YOUR_SALES_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "farmerId": 1,
    "farmerName": "Test Farmer",
    "farmerCode": "TF001",
    "farmerAddress": "Test Address",
    "branchId": 1,
    "lineId": 1,
    "placementQty": 100,
    "placementDate": "2024-01-01T00:00:00Z",
    "details": []
  }'

# Test with StoreManager token (should fail)
curl -X POST "https://localhost:5001/api/Placement" \
  -H "Authorization: Bearer YOUR_STORE_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "farmerId": 1,
    "farmerName": "Test Farmer",
    "farmerCode": "TF001",
    "farmerAddress": "Test Address",
    "branchId": 1,
    "lineId": 1,
    "placementQty": 100,
    "placementDate": "2024-01-01T00:00:00Z",
    "details": []
  }'
```

**Expected:** SalesManager should get 200 OK, StoreManager should get 403 Forbidden.

### 3. Test Unauthorized Access

#### Test without token
```bash
curl -X GET "https://localhost:5001/api/Farmer"
```

**Expected:** 401 Unauthorized

#### Test with invalid token
```bash
curl -X GET "https://localhost:5001/api/Farmer" \
  -H "Authorization: Bearer invalid_token"
```

**Expected:** 401 Unauthorized

### 4. Test Role-Based Filtering

#### Test with query parameters
```bash
# Test pagination
curl -X GET "https://localhost:5001/api/Farmer?PageNumber=1&PageSize=5" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Test search
curl -X GET "https://localhost:5001/api/Farmer?SearchTerm=test" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Test filtering
curl -X GET "https://localhost:5001/api/Farmer?BranchId=1" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Test sorting
curl -X GET "https://localhost:5001/api/Farmer?SortBy=FarmerName&SortDirection=asc" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected:** All should return 200 OK with filtered/sorted data.

## Expected Results Summary

| Endpoint | Method | Admin | SalesManager | StoreManager | PurchaseManager | ReadOnlyManager | UtilityManager |
|----------|--------|-------|--------------|--------------|-----------------|-----------------|----------------|
| `/api/Farmer` | GET | ✅ 200 | ✅ 200 | ✅ 200 | ✅ 200 | ✅ 200 | ❌ 403 |
| `/api/Farmer` | POST | ✅ 201 | ✅ 201 | ✅ 201 | ✅ 201 | ❌ 403 | ❌ 403 |
| `/api/Branch` | GET | ✅ 200 | ✅ 200 | ✅ 200 | ✅ 200 | ✅ 200 | ❌ 403 |
| `/api/Branch` | POST | ✅ 201 | ❌ 403 | ❌ 403 | ❌ 403 | ❌ 403 | ❌ 403 |
| `/api/Placement` | POST | ✅ 200 | ✅ 200 | ❌ 403 | ❌ 403 | ❌ 403 | ❌ 403 |
| `/api/Placement/GetAll` | GET | ✅ 200 | ✅ 200 | ✅ 200 | ✅ 200 | ✅ 200 | ❌ 403 |

## Troubleshooting

### Common Issues

1. **401 Unauthorized**: Check if token is valid and not expired
2. **403 Forbidden**: User doesn't have required role for the operation
3. **500 Internal Server Error**: Check database connection and stored procedures

### Debug Steps

1. **Check User Roles in Database**:
   ```sql
   SELECT u.UM_USERNAME, r.RoleName
   FROM USER_MASTER u
   INNER JOIN UserRoles ur ON u.UM_CODE = ur.UserCode
   INNER JOIN Roles r ON ur.RoleId = r.RoleId
   WHERE u.UM_USERNAME = 'SALESMGR';
   ```

2. **Test SP_GetUserRoles**:
   ```sql
   EXEC SP_GetUserRoles @UserName = 'SALESMGR', @CompanyId = '1';
   ```

3. **Check JWT Token Claims**:
   Use a JWT decoder to verify the token contains the correct role claims.

## Next Steps

1. **Customize Roles**: Modify role assignments based on your business requirements
2. **Add More Users**: Create additional users for each role as needed
3. **Extend Authorization**: Add role-based authorization to more endpoints
4. **Frontend Integration**: Update your frontend to handle role-based UI changes
