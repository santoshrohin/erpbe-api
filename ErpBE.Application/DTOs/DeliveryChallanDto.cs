using ErpBE.Application.Common.Models;

namespace ErpBE.Application.DTOs
{
    public class DeliveryChallanMasterDto
    {
        public int        ChallanCode    { get; set; }  // DCM_CODE
        public int        CompanyCode    { get; set; }  // DCM_CM_CODE
        public int?       CustomerCode   { get; set; }  // DCM_P_CODE
        public string?    CustomerName   { get; set; }  // P_NAME (from PARTY_MASTER join)
        public string?    Type           { get; set; }  // DCM_TYPE
        public decimal?   ChallanNumber  { get; set; }  // DCM_NO
        public DateTime?  ChallanDate    { get; set; }  // DCM_DATE
        public string?    InvoiceNumber  { get; set; }  // DCM_INV_NO
        public string?    Through        { get; set; }  // DCM_THROUGH
        public string?    VehicleNumber  { get; set; }  // DCM_VEH_NO
        public string?    LrNumber       { get; set; }  // DCM_LR_NO
        public string?    OrderNumber    { get; set; }  // DCM_ORDER_NO
        public DateTime?  OrderDate      { get; set; }  // DCM_ORDER_DATE
        public bool       IsDeleted      { get; set; }  // ES_DELETE
        public bool       IsModifyLocked { get; set; }  // MODIFY
        public bool       MaterialType   { get; set; }  // DCM_MAT_TYPE
        public bool       IsReturnable   { get; set; }  // DCM_IS_RETURNABLE

        public List<DeliveryChallanDetailDto> Details { get; set; } = new();
    }

    public class DeliveryChallanDetailDto
    {
        public int      ChallanCode       { get; set; }  // DCD_DCM_CODE
        public int?     ItemCode          { get; set; }  // DCD_I_CODE
        public double   OrderedQuantity   { get; set; }  // DCD_ORD_QTY
        public string?  BatchNumber       { get; set; }  // DCD_BATCH_NO
        public string?  NumberOfPacks     { get; set; }  // DCD_NO_OF_PACKS
        public int?     UomCode           { get; set; }  // DCD_UM_CODE
        public bool     IsDeleted         { get; set; }  // ES_DELETE
        public string?  Remark            { get; set; }  // DCD_REMARK
        public double?  ReturnedQuantity  { get; set; }  // DCD_RET_QTY
    }

    public class CreateDeliveryChallanRequest
    {
        public int        CompanyCode   { get; set; }
        public int?       CustomerCode  { get; set; }
        public string?    Type          { get; set; }
        public DateTime?  ChallanDate   { get; set; }
        public string?    InvoiceNumber { get; set; }
        public string?    Through       { get; set; }
        public string?    VehicleNumber { get; set; }
        public string?    LrNumber      { get; set; }
        public string?    OrderNumber   { get; set; }
        public DateTime?  OrderDate     { get; set; }
        public bool       MaterialType  { get; set; }
        public bool       IsReturnable  { get; set; }

        public List<CreateDeliveryChallanDetailRequest> Details { get; set; } = new();
    }

    public class CreateDeliveryChallanDetailRequest
    {
        public int?     ItemCode        { get; set; }
        public double   OrderedQuantity { get; set; }
        public string?  BatchNumber     { get; set; }
        public string?  NumberOfPacks   { get; set; }
        public int?     UomCode         { get; set; }
        public string?  Remark          { get; set; }
    }

    public class UpdateDeliveryChallanRequest : CreateDeliveryChallanRequest
    {
        public int ChallanCode { get; set; }
    }

    public class UpdateDeliveryChallanDetailRequest : CreateDeliveryChallanDetailRequest
    {
        public double? ReturnedQuantity { get; set; }
    }

    public class DeliveryChallanQueryParameters : QueryParameters
    {
        public int?       CompanyCode  { get; set; }
        public int?       CustomerCode { get; set; }
        public DateTime?  DateFrom     { get; set; }
        public DateTime?  DateTo       { get; set; }
        public string?    SearchText   { get; set; }
    }
}
