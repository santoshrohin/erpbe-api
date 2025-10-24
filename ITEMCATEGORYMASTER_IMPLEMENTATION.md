# Item Category Master Implementation Plan

## Overview
Implementing Item Category Master from legacy ASP.NET Web Forms to Clean Architecture API following UnitMaster standards.

## Legacy Application Analysis

### Table Structure: ITEM_CATEGORY_MASTER
- **I_CAT_CODE** (int, PK) - Category ID
- **I_CAT_CM_COMP_ID** (int) - Company ID
- **I_CAT_NAME** (varchar(50)) - Category Name
- **ES_DELETE** (bit) - Is Deleted (soft delete)
- **MODIFY** (bit) - Modify Lock Flag
- **I_CAT_SHORTCLOSE** (bit) - Auto Short Close Flag

### Business Logic Identified
1. **Validation**:
   - Category Name is required (max 50 characters)
   - Category Name must be unique within a company
   - Company ID is required
   - Category Name is converted to uppercase

2. **Features**:
   - Auto Short Close flag
   - Soft delete (ES_DELETE flag)
   - Modify lock mechanism
   - Search/Filter by category name
   - Pagination support (15 items per page)

3. **Operations**:
   - CREATE: Check uniqueness, insert with company ID
   - READ: Get all, Get by ID, Search by name
   - UPDATE: Check uniqueness (excluding current record), update name and shortclose flag
   - DELETE: Soft delete, check if used in ITEM_MASTER

### Stored Procedures in Legacy
- SP_ITEM_CATEGORY_MASTER_Select (GETINFO, FILLGRID, CHECKSAVE, CHECKUPDATE)
- SP_ITEM_CATEGORY_MASTER_Insert
- SP_ITEM_CATEGORY_MASTER_Update (commented out, using inline SQL)
- SP_ITEM_CATEGORY_MASTER_Delete

## New API Implementation Standards

### Architecture
- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ CQRS Pattern with MediatR
- ✅ Repository Pattern
- ✅ FluentValidation for all validations
- ✅ Stored Procedures with **SET NOCOUNT OFF**

### DTOs Created
1. **ItemCategoryMasterDto** - Response model
2. **CreateItemCategoryMasterRequest** - Create request
3. **UpdateItemCategoryMasterRequest** - Update request
4. **ItemCategoryMasterQueryParameters** - Query parameters with pagination

### Validators Created
1. **CreateItemCategoryMasterRequestValidator**
   - CategoryName: Required, Max 50 chars, Valid characters
   - CompanyId: Greater than 0

2. **UpdateItemCategoryMasterRequestValidator**
   - CategoryId: Greater than 0
   - CategoryName: Required, Max 50 chars, Valid characters

3. **ItemCategoryMasterQueryParametersValidator**
   - PageNumber: Greater than 0
   - PageSize: 1-100
   - SortDirection: ASC or DESC
   - CompanyId: Greater than 0 (if provided)

## Remaining Implementation Tasks

### 1. CQRS Commands
- CreateItemCategoryMasterCommand
- UpdateItemCategoryMasterCommand
- DeleteItemCategoryMasterCommand

### 2. CQRS Queries
- GetItemCategoryMasterByIdQuery
- GetItemCategoryMastersQuery (with pagination, filtering, sorting)
- GetItemCategoryMasterByNameQuery
- CheckItemCategoryNameUniqueQuery

### 3. Command & Query Handlers
- All handlers with repository interaction

### 4. Repository
- IItemCategoryMasterRepository interface
- ItemCategoryMasterRepository implementation

### 5. Stored Procedures (SET NOCOUNT OFF)
- SP_CreateItemCategoryMaster
- SP_UpdateItemCategoryMaster
- SP_DeleteItemCategoryMaster
- SP_GetItemCategoryMasterById
- SP_GetItemCategoryMasters (with pagination, filtering, sorting)
- SP_GetItemCategoryMasterByName
- SP_IsItemCategoryNameUnique

### 6. Controller
- ItemCategoryMasterController with endpoints:
  - POST /api/ItemCategoryMaster - Create
  - PUT /api/ItemCategoryMaster - Update
  - DELETE /api/ItemCategoryMaster/{id} - Delete
  - GET /api/ItemCategoryMaster/{id} - Get by ID
  - GET /api/ItemCategoryMaster - Get all with pagination
  - GET /api/ItemCategoryMaster/name/{name} - Get by name
  - GET /api/ItemCategoryMaster/check-unique - Check uniqueness
  - PATCH /api/ItemCategoryMaster/{id}/status - Toggle active status

### 7. Testing
- Unit tests for validators
- Integration tests for all endpoints
- Test data cleanup after each test
- No production database impact

## Progress
- ✅ DTOs Created (4 files)
- ✅ Validators Created (3 files)
- ⏳ CQRS Commands (0/3)
- ⏳ CQRS Queries (0/4)
- ⏳ Handlers (0/7)
- ⏳ Repository (0/2)
- ⏳ Stored Procedures (0/7)
- ⏳ Controller (0/1)
- ⏳ Tests (0/1)
- ⏳ Service Registration (0/1)

## Next Steps
Continue with CQRS implementation...


