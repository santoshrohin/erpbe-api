# Role-Based Authorization Guide

## Overview
This API implements role-based authorization on top of the existing JWT authentication system. Each user is assigned one or more roles that determine their access to different modules and operations.

## Roles and Permissions

### 1. **Admin** - Full Access
- **Access**: All modules and operations
- **Can**: Create, Read, Update, Delete everything
- **Modules**: All modules (Sales, Store, Purchase, Utility, Farmer, Placement, Reports)

### 2. **SalesManager** - Sales Module Access
- **Access**: Sales-related operations and read access to other modules
- **Can**: Create/Manage placements, view farmers, branches, lines
- **Modules**: Sales, Farmer, Placement (limited), Reports (sales only)

### 3. **StoreManager** - Store Module Access
- **Access**: Store-related operations and read access to other modules
- **Can**: Manage inventory, view placements, farmers
- **Modules**: Store, Farmer, Placement (read-only), Reports (store only)

### 4. **PurchaseManager** - Purchase Module Access
- **Access**: Purchase-related operations and read access to other modules
- **Can**: Manage purchases, view farmers, suppliers
- **Modules**: Purchase, Farmer, Reports (purchase only)

### 5. **ReadOnlyManager** - Read Access Only
- **Access**: Read-only access to entire application
- **Can**: View all data but cannot create, update, or delete
- **Modules**: All modules (read-only)

### 6. **UtilityManager** - Utility Module Access
- **Access**: Utility operations and limited read access
- **Can**: Manage system utilities, configurations
- **Modules**: Utility, Reports (utility only)

## API Endpoints Authorization

### Farmer Management
| Endpoint | Method | Admin | SalesManager | StoreManager | PurchaseManager | ReadOnlyManager | UtilityManager |
|----------|--------|-------|--------------|--------------|-----------------|-----------------|----------------|
| `/api/Farmer` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| `/api/Farmer` | POST | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `/api/Farmer/{id}` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |

### Branch Management
| Endpoint | Method | Admin | SalesManager | StoreManager | PurchaseManager | ReadOnlyManager | UtilityManager |
|----------|--------|-------|--------------|--------------|-----------------|-----------------|----------------|
| `/api/Branch` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| `/api/Branch` | POST | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

### Placement Management
| Endpoint | Method | Admin | SalesManager | StoreManager | PurchaseManager | ReadOnlyManager | UtilityManager |
|----------|--------|-------|--------------|--------------|-----------------|-----------------|----------------|
| `/api/Placement` | POST | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `/api/Placement/GetPlacementDetails` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| `/api/Placement/GetAll` | GET | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |

## Authorization Attributes

### Custom Attributes
```csharp
[AuthorizeAdmin]        // Admin only
[AuthorizeSales]        // Admin, SalesManager
[AuthorizeStore]        // Admin, StoreManager
[AuthorizePurchase]     // Admin, PurchaseManager
[AuthorizeReadOnly]     // Admin, ReadOnlyManager
[AuthorizeUtility]      // Admin, UtilityManager
[AuthorizeManagement]   // Admin, SalesManager, StoreManager, PurchaseManager
```

### Usage Examples
```csharp
[HttpGet]
[AuthorizeReadOnly] // Admin, ReadOnlyManager can read
public async Task<IActionResult> GetAll() { }

[HttpPost]
[AuthorizeSales] // Admin, SalesManager can create
public async Task<IActionResult> Create() { }

[HttpPost]
[AuthorizeAdmin] // Only Admin can create
public async Task<IActionResult> Create() { }
```

## JWT Token Structure

The JWT token now includes role claims:
```json
{
  "sub": "user123",
  "unique_name": "john.doe",
  "CompanyId": "1",
  "role": ["SalesManager", "ReadOnlyManager"]
}
```

## Login Response

The login response now includes user roles:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "john.doe",
  "companyId": 1,
  "companyCode": "001",
  "companyName": "ABC Company",
  "email": "john@abc.com",
  "roles": ["SalesManager"],
  "permissions": []
}
```

## Database Requirements

### Required Stored Procedure
```sql
CREATE PROCEDURE SP_GetUserRoles
    @UserName NVARCHAR(50),
    @CompanyId INT
AS
BEGIN
    -- Return user roles based on your user-role table structure
    SELECT r.RoleName
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    INNER JOIN Users u ON ur.UserId = u.UserId
    WHERE u.UserName = @UserName 
    AND u.CompanyId = @CompanyId
    AND ur.IsActive = 1
    AND r.IsActive = 1;
END
```

### Sample Role Table Structure
```sql
-- Roles table
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    RoleDescription NVARCHAR(255),
    IsActive BIT DEFAULT 1
);

-- UserRoles table
CREATE TABLE UserRoles (
    UserId INT,
    RoleId INT,
    IsActive BIT DEFAULT 1,
    PRIMARY KEY (UserId, RoleId)
);

-- Insert default roles
INSERT INTO Roles (RoleName, RoleDescription) VALUES
('Admin', 'Full system access'),
('SalesManager', 'Sales module access'),
('StoreManager', 'Store module access'),
('PurchaseManager', 'Purchase module access'),
('ReadOnlyManager', 'Read-only access'),
('UtilityManager', 'Utility module access');
```

## Error Responses

### Unauthorized (401)
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required"
}
```

### Forbidden (403)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions for this operation"
}
```

## Testing Authorization

### Test with Different Roles
```bash
# Login as Admin
curl -X POST "https://localhost:5001/api/Login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password","companyId":1,"financialYearCode":2024}'

# Use the token in subsequent requests
curl -X GET "https://localhost:5001/api/Farmer" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Security Considerations

1. **Token Validation**: JWT tokens are validated on every request
2. **Role Claims**: Roles are embedded in JWT tokens for performance
3. **Database Verification**: User roles are verified against database on login
4. **Policy-Based**: Authorization policies are centrally managed
5. **Attribute-Based**: Easy to apply authorization at controller/action level

## Migration Guide

### For Existing Users
1. Assign default roles to existing users
2. Update login stored procedure to return roles
3. Test authorization with different user types
4. Update frontend to handle role-based UI

### For New Features
1. Determine which roles should have access
2. Apply appropriate authorization attributes
3. Test with different user roles
4. Update documentation
