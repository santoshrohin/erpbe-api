# Customer PO (Purchase Order) Module - Analysis

## 📋 Overview

**Module**: Customer PO (Sales Order from Customer)  
**Legacy Files**:
- `CustomerPO.aspx` / `CustomerPO.aspx.cs` - Add/Edit/View form
- `ViewCustomerPO.aspx` / `ViewCustomerPO.aspx.cs` - List view
- `CustomerPORPTForm.aspx` - Print/Report form

**Database Tables**:
- `CUSTPO_MASTER` (46 columns)
- `CUSTPO_DETAIL` (21 columns)

**Total Fields**: 67 columns

---

## 📊 Table Structure

### CUSTPO_MASTER (46 columns)

#### Core PO Information (9 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_CODE` | int | PK - PO Code |
| `CPOM_P_CODE` | int | FK - Party/Customer Code |
| `CPOM_PONO` | varchar(100) | Customer PO Number |
| `CPOM_DOC_NO` | int | Internal Document Number |
| `CPOM_TYPE` | int | PO Type |
| `CPOM_DATE` | datetime | PO Date |
| `CPOM_CR_DAYS` | int | Credit Days |
| `CPOM_CM_COMP_ID` | int | Company ID |
| `CPOM_WORK_ODR_NO` | varchar(50) | Work Order Number |

#### System Fields (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `MODIFY` | bit | Lock for editing (1 = locked) |
| `ES_DELETE` | bit | Soft delete flag |

#### Payment & Terms (3 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_PAY_TERM` | varchar(260) | Payment Terms |
| `CPOM_AUTH_FLG` | bit | Authorization Flag |
| `CPOM_PO_DATE` | datetime | Customer PO Date |

#### Quotation Reference (1 column)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_QE_CODE` | int | Quotation/Enquiry Code |

#### Tax Information (6 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_T_NAME` | varchar(50) | Tax Name |
| `CPOM_T_PER` | float | Tax Percentage |
| `CPOM_T_AMT` | float | Tax Amount |
| `CPOM_EXC_PER` | float | Excise Percentage (Legacy) |
| `CPOM_EXC_EDU_PER` | float | Education Cess % (Legacy) |
| `CPOM_EXC_HEDU_PER` | float | Higher Education Cess % (Legacy) |

#### Amount Fields (8 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_BASIC_AMT` | float | Basic Amount |
| `CPOM_DISCOUNT_PER` | float | Discount Percentage |
| `CPOM_DISCOUNT_AMT` | float | Discount Amount |
| `CPOM_DISCOUNT_REASON` | varchar(50) | Discount Reason |
| `CPOM_DEVIATION_AMT` | float | Deviation Amount |
| `CPOM_DEVIATION_REASON` | varchar(50) | Deviation Reason |
| `CPOM_PACKING_AMT` | float | Packing Amount |
| `CPOM_EXC_AMT` | float | Excise Amount (Legacy) |

#### Totals (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_ROUNDING` | float | Rounding Amount |
| `CPOM_GRAND_TOT` | float | Grand Total |

#### Invoice Reference (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_INV_FLAG` | bit | Invoice Generated Flag |
| `CPOM_AM_COUNT` | int | Amendment Count |

#### Export Information (7 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_FINAL_DEST` | varchar(50) | Final Destination |
| `CPOM_PRE_CARR_BY` | varchar(50) | Pre-Carriage By |
| `CPOM_PORT_LOAD` | varchar(50) | Port of Loading |
| `CPOM_PORT_DIS` | varchar(50) | Port of Discharge |
| `CPOM_PLACE_DEL` | varchar(50) | Place of Delivery |
| `CPOM_BUYER_NAME` | varchar(50) | Buyer Name |
| `CPOM_BUYER_ADD` | varchar(150) | Buyer Address |

#### Currency (1 column)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_CURR_CODE` | int | Currency Code |

#### Amendment (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_AM_DATE` | datetime | Amendment Date |
| `CPOM_INQ_CODE` | int | Inquiry Code |

#### Miscellaneous (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOM_IS_VERBAL` | bit | Is Verbal Order |
| `CPOM_PROJECT_CODE` | int | Project Code |
| `CPOM_PROJECT_NAME` | varchar(100) | Project Name |

---

### CUSTPO_DETAIL (21 columns)

#### Core Item Information (7 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_CPOM_CODE` | int | FK - Master PO Code |
| `CPOD_I_CODE` | int | Item Code |
| `CPOD_UOM_CODE` | int | Unit of Measurement |
| `CPOD_ORD_QTY` | float | Ordered Quantity |
| `CPOD_RATE` | float | Rate per Unit |
| `CPOD_AMT` | float | Amount (Qty * Rate) |
| `CPOD_DESC` | varchar(max) | Item Description |

#### Customer Item Reference (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_CUST_I_CODE` | varchar(max) | Customer Item Code |
| `CPOD_CUST_I_NAME` | varchar(max) | Customer Item Name |

#### Status & Dispatch (3 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_STATUS` | int | Status Code |
| `CPOD_DISPACH` | float | Dispatched Quantity |
| `CPOD_IS_ORDER` | bit | Is Order Flag |

#### Store & Currency (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_ST_CODE` | int | Store Code |
| `CPOD_CURR_CODE` | int | Currency Code |

#### Work Order (1 column)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_WO_QTY` | float | Work Order Quantity |

#### Modification Tracking (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_MODNO` | varchar(50) | Modification Number |
| `CPOD_MODDATE` | datetime | Modification Date |

#### Amortization (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_AMORTRATE` | float | Amortization Rate |
| `CPOD_DIEAMORTRATE` | float | Die Amortization Rate |

#### Discount (2 columns)
| Column | Type | Description |
|--------|------|-------------|
| `CPOD_DISC_PER` | float | Discount Percentage |
| `CPOD_DISC_AMT` | float | Discount Amount |

---

## 🔄 Business Process Flow

### 1. **Create Customer PO**
```
Customer → Places Order → Create PO in System
├─ Select Customer
├─ Enter PO Number (from customer)
├─ Select PO Type (Order/Enquiry)
├─ Enter PO Date
├─ Enter Work Order Number (optional)
├─ Select Project Code
├─ Select Quotation/Enquiry (optional)
├─ Enter Payment Terms
├─ Add Line Items:
│  ├─ Select Item
│  ├─ Enter Quantity
│  ├─ Enter Rate
│  ├─ Enter Customer Item Code/Name
│  └─ Calculate Amount
├─ Apply Discount (if any)
├─ Calculate Tax
├─ Calculate Grand Total
└─ Save PO
```

### 2. **View/Search Customer POs**
```
ViewCustomerPO.aspx
├─ List all POs for company
├─ Search by:
│  ├─ PO Number
│  ├─ Customer Name
│  ├─ Date
│  ├─ Work Order Number
│  ├─ Customer Item Code
│  └─ Document Number
├─ Display columns:
│  ├─ PO Code
│  ├─ PO Number
│  ├─ Date
│  ├─ Customer Name
│  ├─ Work Order Number
│  ├─ Amendment Status
│  └─ Customer Item Code
└─ Actions:
   ├─ View
   ├─ Modify
   ├─ Amend
   └─ Delete
```

### 3. **Amend Customer PO**
```
Existing PO → Amend
├─ Increment Amendment Count
├─ Update Amendment Date
├─ Modify Items/Quantities/Rates
├─ Recalculate Totals
└─ Save as Amendment
```

### 4. **Convert Enquiry to PO**
```
Enquiry → Convert to PO
├─ Load Enquiry Details
├─ Pre-fill Customer Information
├─ Pre-fill Items from Enquiry
├─ User can modify
└─ Save as Customer PO
```

### 5. **Generate Tax Invoice from PO**
```
Customer PO → Generate Invoice
├─ Link Invoice to PO (CPOM_INV_FLAG = 1)
├─ Copy items to Invoice
├─ Update dispatch quantities
└─ Create Tax Invoice
```

---

## 🎯 Key Validations (from Legacy Code)

### Form-Level Validations
1. ✅ **PO Type is required**
   - Cannot proceed without selecting PO type
2. ✅ **Project Code is required**
   - Must select a project code
3. ✅ **Customer is required**
   - Must select a customer
4. ✅ **PO Number is required**
   - Customer PO number must be entered
5. ✅ **PO Date is required**
   - PO date must be valid
6. ✅ **At least one line item required**
   - Cannot save PO without items

### Line Item Validations
1. ✅ **Item is required**
   - Must select an item
2. ✅ **Quantity > 0**
   - Ordered quantity must be greater than zero
3. ✅ **Rate > 0**
   - Rate must be greater than zero
4. ✅ **UOM is required**
   - Unit of measurement must be selected

### Business Rules
1. **Amendment Tracking**
   - Each amendment increments `CPOM_AM_COUNT`
   - Amendment date is recorded
   - Original PO is preserved

2. **Invoice Linkage**
   - When invoice is generated, `CPOM_INV_FLAG` is set to 1
   - Prevents duplicate invoicing

3. **Dispatch Tracking**
   - `CPOD_DISPACH` tracks dispatched quantity
   - Should not exceed `CPOD_ORD_QTY`

4. **Verbal Orders**
   - `CPOM_IS_VERBAL` flag indicates verbal/phone orders
   - May require confirmation later

5. **Customer Item Mapping**
   - Customer may have their own item codes
   - Store in `CPOD_CUST_I_CODE` and `CPOD_CUST_I_NAME`

---

## 📊 Database Relationships

```
CUSTPO_MASTER
├─ PARTY_MASTER (CPOM_P_CODE → P_CODE) - Customer
├─ PROJECT_CODE_MASTER (CPOM_PROJECT_CODE → PROCM_CODE) - Project
├─ CURRENCY_MASTER (CPOM_CURR_CODE → CURR_CODE) - Currency
└─ CUSTPO_DETAIL (CPOM_CODE → CPOD_CPOM_CODE) - One-to-Many

CUSTPO_DETAIL
├─ CUSTPO_MASTER (CPOD_CPOM_CODE → CPOM_CODE) - Master
├─ ITEM_MASTER (CPOD_I_CODE → I_CODE) - Item
├─ UNIT_MASTER (CPOD_UOM_CODE → UM_CODE) - UOM
├─ STORE_MASTER (CPOD_ST_CODE → STM_CODE) - Store
└─ CURRENCY_MASTER (CPOD_CURR_CODE → CURR_CODE) - Currency

CUSTPO_MASTER → INVOICE_MASTER (via CPOM_INV_FLAG and INM_CPOM_CODE)
```

---

## 🔍 Search & Filter Capabilities

### Search Fields (from ViewCustomerPO.aspx.cs)
The legacy application searches across:
1. ✅ PO Number (`CPOM_PONO`)
2. ✅ Date (`CPOM_DATE`)
3. ✅ Customer Name (`P_NAME`)
4. ✅ Document Number (`CPOM_DOC_NO`)
5. ✅ Work Order Number (`CPOM_WORK_ODR_NO`)
6. ✅ Customer Item Code (`CPOD_CUST_I_CODE`)

### Display Columns in List View
1. PO Code (`CPOM_CODE`)
2. PO Number (`CPOM_PONO`)
3. Date (`CPOM_DATE`)
4. Customer Name (`P_NAME`)
5. Work Order Number (`CPOM_WORK_ODR_NO`)
6. Amendment Status (`CPOM_AM_COUNT` - Yes/No)
7. Customer Item Code (`CPOD_CUST_I_CODE`)

---

## 💡 Recommendations for Implementation

### 1. **DTOs to Create**
- `CustomerPoMasterDto` (46 fields)
- `CustomerPoDetailDto` (21 fields)
- `CreateCustomerPoRequest` (master + details)
- `UpdateCustomerPoRequest` (master + details)
- `CustomerPoQueryParameters` (pagination, filtering, sorting)
- `CustomerPoPagedResponse` (paged results)

### 2. **CQRS Components**
**Commands**:
- `CreateCustomerPoCommand`
- `UpdateCustomerPoCommand`
- `DeleteCustomerPoCommand`
- `AmendCustomerPoCommand` (special case)

**Queries**:
- `GetCustomerPoByIdQuery`
- `GetAllCustomerPosQuery` (with filters)
- `GetCustomerPosByCustomerQuery`
- `GetPoItemsByPoCodeQuery`

### 3. **Stored Procedures to Create**
- `ERP_CreateCustomerPo` - Insert master
- `ERP_CreateCustomerPoDetail` - Insert detail
- `ERP_UpdateCustomerPo` - Update master
- `ERP_DeleteCustomerPo` - Soft delete (master + details)
- `ERP_GetCustomerPoById` - Get by ID with details
- `ERP_GetAllCustomerPos` - Paged list with filters
- `ERP_AmendCustomerPo` - Amendment logic
- `ERP_GeneratePoNumber` - Auto numbering

### 4. **Key Validations to Implement**
- PO Type required
- Project Code required
- Customer required
- PO Number required (unique per customer?)
- At least one line item
- Quantity > 0
- Rate > 0
- Dispatch qty ≤ Ordered qty

### 5. **Special Features**
- **Amendment Tracking**: Increment count, record date
- **Enquiry Conversion**: Convert enquiry to PO
- **Invoice Linkage**: Track if invoice generated
- **Customer Item Mapping**: Store customer's item codes
- **Verbal Order Flag**: Track verbal vs written orders

---

## 📋 Implementation Checklist

### Phase 1: Database & DTOs
- [ ] Create all DTOs (Master, Detail, Request, Response)
- [ ] Create Query Parameters DTO
- [ ] Map all 67 fields correctly

### Phase 2: CQRS Components
- [ ] Create Commands (Create, Update, Delete, Amend)
- [ ] Create Queries (GetById, GetAll, etc.)
- [ ] Create Command Handlers
- [ ] Create Query Handlers

### Phase 3: Validation
- [ ] Create Command Validators (all validations)
- [ ] Create Query Validators
- [ ] Implement business rules

### Phase 4: Stored Procedures
- [ ] Create 8 stored procedures
- [ ] Deploy to database
- [ ] Test each procedure

### Phase 5: Repository & Controller
- [ ] Create `ICustomerPoRepository`
- [ ] Implement `CustomerPoRepository` (Dapper)
- [ ] Create `CustomerPoController` (API endpoints)
- [ ] Register services in DI

### Phase 6: Testing
- [ ] Create integration tests (CRUD operations)
- [ ] Create validator tests (all validations)
- [ ] Test amendment logic
- [ ] Test invoice linkage

---

## 🎯 Success Criteria

✅ **Complete CRUD** operations  
✅ **All 67 fields** implemented  
✅ **Amendment tracking** working  
✅ **Enquiry conversion** functional  
✅ **Search & filter** working  
✅ **Validations** from legacy app  
✅ **Transaction management** (master + details atomic)  
✅ **100% test coverage**  
✅ **Clean Architecture** compliance  
✅ **CQRS pattern** followed  

---

## 📝 Notes

1. **Export Fields**: 7 export-related columns exist - verify if needed
2. **Legacy Tax Fields**: Excise fields are pre-GST - may need GST fields
3. **Amendment Logic**: Important feature - needs careful implementation
4. **Invoice Linkage**: Critical for preventing duplicate invoices
5. **Dispatch Tracking**: Important for production planning

---

**Ready to implement Customer PO module following the same patterns as Tax Invoice!**

