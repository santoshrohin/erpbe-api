# Customer Master - Actual Column Mapping

## PARTY_MASTER Table Structure (47 columns)

| # | Column Name | Data Type | Nullable | Usage |
|---|---|---|---|---|
| 1 | P_CODE | int | NOT NULL | Primary Key |
| 2 | P_CM_COMP_ID | int | NULL | Company ID |
| 3 | P_TYPE | smallint | NOT NULL | Party Type (1=Customer, 2=Supplier) |
| 4 | P_NAME | varchar(-1) | NOT NULL | Party Name |
| 5 | P_CONTACT | varchar(75) | NOT NULL | Contact Person |
| 6 | P_PARTY_CODE | int | NULL | Auto-generated Party Code |
| 7 | P_VEND_CODE | varchar(30) | NULL | Vendor Code |
| 8 | P_ADD1 | varchar(255) | NOT NULL | Address |
| 9 | P_CITY | varchar(50) | NULL | City |
| 10 | P_DISTRICT | varchar(50) | NULL | District |
| 11 | P_PIN_CODE | varchar(15) | NULL | Pin Code |
| 12 | P_PHONE | varchar(50) | NULL | Phone |
| 13 | P_MOB | varchar(55) | NULL | Mobile |
| 14 | P_SCT_CODE | int | NULL | Sector Code (legacy) |
| 15 | P_FAX | varchar(50) | NULL | Fax Number |
| 16 | P_EMAIL | varchar(100) | NULL | Email |
| 17 | P_PAN | varchar(25) | NULL | PAN Number |
| 18 | P_CST | varchar(50) | NULL | CST Number |
| 19 | P_VAT | varchar(50) | NULL | VAT/TIN Number |
| 20 | P_SER_TAX_NO | varchar(50) | NULL | Service Tax Number |
| 21 | P_ECC_NO | varchar(50) | NULL | ECC Number |
| 22 | P_CATEGORY | smallint | NULL | Category |
| 23 | P_EXC_DIV | varchar(50) | NULL | Excise Division |
| 24 | P_EXC_RANGE | varchar(50) | NULL | Excise Range |
| 25 | P_EXC_COLLECTORATE | varchar(50) | NULL | Excise Collectorate |
| 26 | P_TALLY | varchar(-1) | NULL | Tally Name |
| 27 | ES_DELETE | bit | NULL | Soft Delete Flag |
| 28 | MODIFY | bit | NULL | Modify Lock |
| 29 | P_ACTIVE_IND | bit | NULL | Active Indicator |
| 30 | P_INHOUSE_IND | bit | NULL | In-house Indicator |
| 31 | P_LBT_NO | varchar(50) | NULL | GST/LBT Number |
| 32 | P_LBT_IND | bit | NULL | LBT Applicable |
| 33 | P_CUST_TYPE | varchar(20) | NULL | Customer Type |
| 34 | P_SM_CODE | int | NULL | Sales Manager Code |
| 35 | P_CITY_CODE | int | NULL | City Code |
| 36 | P_COORDINATOR | varchar(100) | NULL | Coordinator |
| 37 | P_COORDINATOR_EMAIL | varchar(100) | NULL | Coordinator Email |
| 38 | P_DELIVERY_ADD | varchar(150) | NULL | Delivery Address |
| 39 | P_NOTE | varchar(200) | NULL | Notes/Remarks |
| 40 | P_CREDITDAYS | int | NULL | Credit Days |
| 41 | P_TDS | float | NULL | TDS Percentage |
| 42 | P_E_CODE | int | NULL | Employee Code |
| 43 | P_A_CODE | int | NULL | Area Code (FK to AREA_MASTER) |
| 44 | P_COUNTRY_CODE | int | NULL | Country Code |
| 45 | P_STM_CODE | int | NULL | State Code |
| 46 | P_ABBREVATION | varchar(20) | NULL | Abbreviation |
| 47 | P_GST_NO | varchar(50) | NULL | GST Number |

## Related Tables

### AREA_MASTER
- Primary Key: A_CODE
- Display Column: A_DESC
- Company FK: A_CM_COMP_ID

### CUSTOMER_TYPE_MASTER  
- Primary Key: CTM_CODE
- Display Column: CTM_TYPE_DESC
- Type Code: CTM_TYPE_CODE
- Company FK: CTM_CM_COMP_ID

### Transaction Tables
- INVOICE_MASTER.INM_P_CODE references PARTY_MASTER.P_CODE

## DTO Mapping (What we need to expose)

| DTO Property | PARTY_MASTER Column | Notes |
|---|---|---|
| Id | P_CODE | |
| CompanyId | P_CM_COMP_ID | |
| PartyCode | P_PARTY_CODE | Auto-generated (MAX + 1) |
| PartyName | P_NAME | Required |
| ContactPerson | P_CONTACT | |
| Abbreviation | P_ABBREVATION | Uppercase |
| Address | P_ADD1 | |
| City | P_CITY | |
| District | P_DISTRICT | |
| Phone | P_PHONE | |
| Mobile | P_MOB | |
| Email | P_EMAIL | |
| FaxNo | P_FAX | |
| PinCode | P_PIN_CODE | |
| AreaCode | P_A_CODE | Required, FK to AREA_MASTER |
| AreaName | A_DESC | From AREA_MASTER |
| CustomerType | P_CUST_TYPE | Required |
| CustomerTypeName | CTM_TYPE_DESC | From CUSTOMER_TYPE_MASTER |
| CountryCode | P_COUNTRY_CODE | |
| StateCode | P_STM_CODE | |
| CityCode | P_CITY_CODE | |
| VatTinNo | P_VAT | |
| CstNo | P_CST | |
| GstNo | P_GST_NO | Required if P_LBT_IND = 1 |
| LbtNo | P_LBT_NO | Legacy GST field |
| PanNo | P_PAN | |
| ServiceTaxNo | P_SER_TAX_NO | |
| TallyName | P_TALLY | |
| CreditDays | P_CREDITDAYS | |
| IsLbtApplicable | P_LBT_IND | |
| Remark | P_NOTE | |
| IsActive | P_ACTIVE_IND | |

## Fields NOT in Database (Remove from DTOs)
- Website
- OpeningBalance
- OpeningBalanceType
- CreditLimit
- BankName
- BankAccountNo
- BankBranchName
- BankIfscCode
- IsSezCustomer
- IsCompositeDealer
- CreatedDate (no audit columns in PARTY_MASTER)
- ModifiedDate (no audit columns in PARTY_MASTER)

