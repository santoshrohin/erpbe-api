# Repository Inline Query Violations Report

## 🎯 Purpose
This document tracks all inline SQL queries found in repository classes that violate the architectural standard: **"All database operations MUST use stored procedures"**.

---

## 📊 Test Results

### Architecture Test Status: ❌ **3 FAILURES (Expected)**

| Test | Status | Violations Found |
|------|--------|------------------|
| `Repositories_ShouldNotContainInlineSelectQueries` | ❌ FAIL | 3 |
| `Repositories_ShouldNotContainInlineUpdateQueries` | ❌ FAIL | 2 |
| `Repositories_ShouldNotContainInlineDeleteQueries` | ❌ FAIL | 1 |
| `Repositories_ShouldNotContainInlineInsertQueries` | ✅ PASS | 0 |
| `Repositories_ShouldOnlyUseStoredProcedures` | ✅ PASS | 0 |

---

## ❌ Violations Detected

### **TaxInvoiceRepository** - 5 Violations

#### 1. DELETE Query (Line 422)
```csharp
await connection.ExecuteAsync(
    "DELETE FROM INVOICE_DETAIL WHERE IND_INM_CODE = @InvoiceCode", 
    new { InvoiceCode = invoice.InvoiceCode }, 
    transaction: transaction);
```
**Location**: `UpdateTaxInvoiceAsync` method  
**Purpose**: Delete existing invoice details before inserting new ones  
**Fix Required**: Create stored procedure `ERP_DeleteInvoiceDetails`

---

#### 2. SELECT Query (Line 569)
```csharp
var isLocked = await connection.QueryFirstOrDefaultAsync<bool>(
    "SELECT ISNULL(MODIFY, 0) FROM INVOICE_MASTER WHERE INM_CODE = @InvoiceCode",
    new { InvoiceCode = invoiceCode });
```
**Location**: `IsInvoiceLockedAsync` method  
**Purpose**: Check if invoice is locked for editing  
**Fix Required**: Create stored procedure `ERP_CheckInvoiceLock`

---

#### 3. UPDATE Query (Line 580)
```csharp
var result = await connection.ExecuteAsync(
    "UPDATE INVOICE_MASTER SET MODIFY = 1 WHERE INM_CODE = @InvoiceCode",
    new { InvoiceCode = invoiceCode });
```
**Location**: `LockInvoiceAsync` method  
**Purpose**: Lock invoice for editing  
**Fix Required**: Create stored procedure `ERP_LockInvoice`

---

#### 4. UPDATE Query (Line 591)
```csharp
var result = await connection.ExecuteAsync(
    "UPDATE INVOICE_MASTER SET MODIFY = 0 WHERE INM_CODE = @InvoiceCode",
    new { InvoiceCode = invoiceCode });
```
**Location**: `UnlockInvoiceAsync` method  
**Purpose**: Unlock invoice after editing  
**Fix Required**: Create stored procedure `ERP_UnlockInvoice`

---

#### 5. SELECT Query (Line 616)
```csharp
var availableQuantity = await connection.QueryFirstOrDefaultAsync<double>(
    "SELECT ISNULL(I_QTY, 0) FROM ITEM_MASTER WHERE I_CODE = @ItemCode AND I_CM_CODE = @CompanyCode",
    new { ItemCode = itemCode, CompanyCode = companyCode });
```
**Location**: `ValidateItemStockAsync` method  
**Purpose**: Validate available stock quantity  
**Fix Required**: Create stored procedure `ERP_GetItemStock`  
**Note**: This validation logic might need to be updated to use `STOCK_LEDGER` instead of `ITEM_MASTER`

---

### **UserManagementRepository** - 1 Violation

#### 6. SELECT Query (Line 282)
```csharp
var user = await connection.QueryFirstOrDefaultAsync<(string Username, int CompanyId)>(
    "SELECT UM_USERNAME, UM_CM_ID FROM USER_MASTER WHERE UM_CODE = @UserId",
    new { UserId = userId });
```
**Location**: `GetUserBasicInfoAsync` method (assumed)  
**Purpose**: Get basic user information  
**Fix Required**: Create stored procedure `ERP_GetUserBasicInfo`

---

## 🔧 Required Fixes

### Stored Procedures to Create

1. **`ERP_DeleteInvoiceDetails`**
   - Purpose: Delete invoice details by invoice code
   - Parameters: `@InvoiceCode INT`
   - Returns: Number of rows affected

2. **`ERP_CheckInvoiceLock`**
   - Purpose: Check if invoice is locked
   - Parameters: `@InvoiceCode INT`
   - Returns: `BIT` (0 = unlocked, 1 = locked)

3. **`ERP_LockInvoice`**
   - Purpose: Lock invoice for editing
   - Parameters: `@InvoiceCode INT`
   - Returns: Number of rows affected

4. **`ERP_UnlockInvoice`**
   - Purpose: Unlock invoice after editing
   - Parameters: `@InvoiceCode INT`
   - Returns: Number of rows affected

5. **`ERP_GetItemStock`**
   - Purpose: Get available stock for an item
   - Parameters: `@ItemCode INT`, `@CompanyCode INT`
   - Returns: `FLOAT` (available quantity)
   - **Note**: Should query `STOCK_LEDGER` instead of `ITEM_MASTER`

6. **`ERP_GetUserBasicInfo`**
   - Purpose: Get basic user information
   - Parameters: `@UserId INT`
   - Returns: Username and Company ID

---

## 📝 Implementation Steps

### Phase 1: TaxInvoiceRepository
1. ✅ Create `ERP_DeleteInvoiceDetails.sql`
2. ✅ Create `ERP_CheckInvoiceLock.sql`
3. ✅ Create `ERP_LockInvoice.sql`
4. ✅ Create `ERP_UnlockInvoice.sql`
5. ✅ Create `ERP_GetItemStock.sql` (using `STOCK_LEDGER`)
6. ✅ Deploy all procedures to database
7. ✅ Update `TaxInvoiceRepository.cs` to use new SPs
8. ✅ Run tests to verify compliance

### Phase 2: UserManagementRepository
1. ✅ Create `ERP_GetUserBasicInfo.sql`
2. ✅ Deploy procedure to database
3. ✅ Update `UserManagementRepository.cs` to use new SP
4. ✅ Run tests to verify compliance

---

## 🎯 Success Criteria

All 5 architecture tests must pass:
- ✅ No inline SELECT queries
- ✅ No inline INSERT queries
- ✅ No inline UPDATE queries
- ✅ No inline DELETE queries
- ✅ Only `CommandType.StoredProcedure` usage

---

## 📊 Current vs. Target

| Metric | Current | Target |
|--------|---------|--------|
| Inline Queries | 6 | 0 |
| Repositories with Violations | 2 | 0 |
| Architecture Tests Passing | 2/5 (40%) | 5/5 (100%) |
| Compliance Rate | 0% | 100% |

---

## 🚀 Benefits

Once all violations are fixed:
- ✅ **Consistency**: All database operations use stored procedures
- ✅ **Maintainability**: Easier to find and modify database logic
- ✅ **Performance**: Better query plan caching
- ✅ **Security**: Protection against SQL injection
- ✅ **Standards Compliance**: Enforced via automated tests
- ✅ **Testability**: Architecture tests prevent future violations

---

## 📌 Notes

1. The architecture tests will **automatically detect** any new inline queries added in the future
2. All new repositories MUST pass these tests before deployment
3. The tests scan actual source files, not compiled assemblies
4. Tests detect SELECT, INSERT, UPDATE, DELETE, and CommandType.Text usage
5. Stored procedure names containing `ERP_` or `SP_` are excluded from violation detection

---

**Status**: ❌ **VIOLATIONS DETECTED - ACTION REQUIRED**  
**Next Step**: Create missing stored procedures and refactor repositories

