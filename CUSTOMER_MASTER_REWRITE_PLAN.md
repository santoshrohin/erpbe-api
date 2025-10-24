# Customer Master - Complete Rewrite Plan

## Based on Legacy Code Analysis

### Fields Used in Legacy INSERT (Line 553)
```sql
INSERT INTO PARTY_MASTER (
    P_EXC_RANGE,        -- Excise Range
    P_PARTY_CODE,       -- Auto-generated (MAX + 1)
    P_TYPE,             -- Fixed: 1 (Customer)
    P_CM_COMP_ID,       -- Company ID (from session)
    P_NAME,             -- Party Name (REQUIRED, UNIQUE case-insensitive)
    P_CONTACT,          -- Contact Person
    P_VEND_CODE,        -- Vendor Code  
    P_ADD1,             -- Address
    P_COUNTRY_CODE,     -- Country dropdown
    P_CITY_CODE,        -- City dropdown
    P_SM_CODE,          -- State dropdown
    P_PIN_CODE,         -- Pin Code
    P_PHONE,            -- Phone
    P_MOB,              -- Mobile
    P_CATEGORY,         -- Category dropdown
    P_A_CODE,           -- Area dropdown (REQUIRED)
    P_FAX,              -- Fax
    P_EMAIL,            -- Email
    P_PAN,              -- PAN Number
    P_CST,              -- CST Number
    P_VAT,              -- VAT/TIN Number
    P_SER_TAX_NO,       -- Service Tax Number
    P_ECC_NO,           -- ECC Number
    P_E_CODE,           -- Employee/Excise Type dropdown
    P_EXC_DIV,          -- Excise Division
    P_EXC_COLLECTORATE, -- Excise Collectorate
    P_TALLY,            -- Tally Name
    P_ACTIVE_IND,       -- Active checkbox
    P_TDS,              -- TDS Tax percentage (double)
    P_LBT_IND,          -- LBT Applicable checkbox
    P_LBT_NO,           -- LBT/GST Number
    P_CREDITDAYS,       -- Credit Days
    P_CUST_TYPE,        -- Customer Type dropdown (CTM_CODE) - **varchar(20)**
    P_ABBREVATION       -- Abbreviation (UPPERCASE, OPTIONAL, UNIQUE)
)
```

### Fields NOT Used in Legacy (From PARTY_MASTER table)
- P_CODE (auto-increment PK)
- P_VEND_CODE (vendor code - not used in customer)
- P_CITY (city text field - not used)
- P_DISTRICT (district - not used)
- P_SCT_CODE (sector - legacy, not used)
- P_INHOUSE_IND (in-house indicator - not used)
- P_COORDINATOR (coordinator - not used in INSERT)
- P_COORDINATOR_EMAIL (coordinator email - not used in INSERT)
- P_DELIVERY_ADD (delivery address - not used in INSERT)
- P_NOTE (notes - not used in INSERT)
- P_STM_CODE (state master code - using P_SM_CODE instead)
- P_GST_NO (new GST - using P_LBT_NO instead)
- ES_DELETE (soft delete flag - set to 0/False on INSERT)
- MODIFY (modify lock - set by separate function)

### Validation Rules (From CheckValid + Legacy Code)
1. **Required:**
   - P_NAME (Customer Name) - Line 698
   - P_A_CODE (Area) - UI marked required (Line 93)
   - P_CUST_TYPE (Customer Type) - UI marked required (Line 119)

2. **Unique Checks:**
   - P_NAME: Case-insensitive unique (Line 526)
   - P_ABBREVATION: Case-insensitive unique if provided (Lines 534-540)

3. **Conditional:**
   - P_LBT_NO: Required if P_LBT_IND = True (Lines 83-94, error: "Enter GST NO.")

4. **Data Processing:**
   - P_NAME: Escape single quotes (Line 509)
   - P_ADD1: Escape single quotes (Line 511)
   - P_CONTACT: Escape single quotes (Line 512)
   - P_TALLY: Escape single quotes (Line 515)
   - P_ABBREVATION: Convert to UPPERCASE (Line 553)
   - P_PARTY_CODE: Auto-generate as MAX(P_PARTY_CODE) + 1 (Lines 545-550)

### IMPORTANT TYPE MAPPINGS
- P_CUST_TYPE: **varchar(20)** NOT int (stores CTM_CODE as string)
- P_CATEGORY: **smallint**
- P_TDS: **float**
- All checkboxes (P_ACTIVE_IND, P_LBT_IND): **bit**

### DTO Structure (What to expose)
```csharp
public class CustomerMasterDto
{
    public int Id { get; set; }                     // P_CODE
    public int? CompanyId { get; set; }             // P_CM_COMP_ID
    public int? PartyCode { get; set; }             // P_PARTY_CODE
    public string PartyName { get; set; }           // P_NAME
    public string? ContactPerson { get; set; }      // P_CONTACT
    public string? Abbreviation { get; set; }       // P_ABBREVATION
    public string? VendorCode { get; set; }         // P_VEND_CODE
    public string? Address { get; set; }            // P_ADD1
    public string? Phone { get; set; }              // P_PHONE
    public string? Mobile { get; set; }             // P_MOB
    public string? Email { get; set; }              // P_EMAIL
    public string? FaxNo { get; set; }              // P_FAX
    public string? PinCode { get; set; }            // P_PIN_CODE
    
    public int? AreaCode { get; set; }              // P_A_CODE (FK)
    public string? AreaName { get; set; }           // From AREA_MASTER.A_DESC
    
    public string? CustomerType { get; set; }       // P_CUST_TYPE (varchar!)
    public string? CustomerTypeName { get; set; }   // From CUSTOMER_TYPE_MASTER.CTM_TYPE_DESC
    
    public int? CountryCode { get; set; }           // P_COUNTRY_CODE
    public int? StateCode { get; set; }             // P_SM_CODE
    public int? CityCode { get; set; }              // P_CITY_CODE
    public int? CategoryCode { get; set; }          // P_CATEGORY
    public int? EmployeeCode { get; set; }          // P_E_CODE
    
    public string? PanNo { get; set; }              // P_PAN
    public string? CstNo { get; set; }              // P_CST
    public string? VatNo { get; set; }              // P_VAT
    public string? ServiceTaxNo { get; set; }       // P_SER_TAX_NO
    public string? EccNo { get; set; }              // P_ECC_NO
    public string? LbtNo { get; set; }              // P_LBT_NO (GST)
    
    public string? ExciseRange { get; set; }        // P_EXC_RANGE
    public string? ExciseDivision { get; set; }     // P_EXC_DIV
    public string? ExciseCollectorate { get; set; } // P_EXC_COLLECTORATE
    
    public string? TallyName { get; set; }          // P_TALLY
    public int? CreditDays { get; set; }            // P_CREDITDAYS
    public double? TdsPercentage { get; set; }      // P_TDS
    
    public bool? IsActive { get; set; }             // P_ACTIVE_IND
    public bool? IsLbtApplicable { get; set; }      // P_LBT_IND
}
```

### Stored Procedures to Create
1. `ERP_CreateCustomerMaster` - INSERT with auto-generated P_PARTY_CODE
2. `ERP_UpdateCustomerMaster` - UPDATE by P_CODE
3. `ERP_DeleteCustomerMaster` - Soft delete (SET ES_DELETE = 1)
4. `ERP_GetCustomerMasterById` - SELECT with joins
5. `ERP_GetCustomerMasters` - Paginated list with filters
6. `ERP_IsPartyNameUnique` - Check name uniqueness
7. `ERP_IsAbbreviationUnique` - Check abbreviation uniqueness
8. `ERP_IsCustomerModified` - Check MODIFY lock
9. `ERP_CheckCustomerUsage` - Check if used in INVOICE_MASTER

### Next Steps
1. Create DTOs matching exact structure above
2. Create Commands/Queries with correct types (CustomerType as string!)
3. Create Validators matching legacy rules
4. Create Handlers with exact business logic
5. Create Repository with Dapper mappings
6. Create all 9 stored procedures with exact column names
7. Create Controller
8. Create Tests
9. Deploy and test

