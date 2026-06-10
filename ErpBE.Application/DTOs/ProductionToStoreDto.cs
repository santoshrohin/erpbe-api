using ErpBE.Application.Common.Models;

namespace ErpBE.Application.DTOs
{
    public class ProductionToStoreMasterDto
    {
        public int      ProductionCode { get; set; }  // PS_CODE
        public decimal  GinNumber      { get; set; }  // PS_GIN_NO
        public DateTime GinDate        { get; set; }  // PS_GIN_DATE
        public string?  Type           { get; set; }  // PS_TYPE
        public string?  PersonName     { get; set; }  // PS_PERSON_NAME
        public decimal? MrCode         { get; set; }  // PS_MR_CODE
        public int?     CustomerCode   { get; set; }  // PS_P_CODE
        public int?     BatchNo        { get; set; }  // PS_BATCH_NO
        public int      CompanyCode    { get; set; }  // PS_CM_COMP_CODE
        public bool     IsLocked       { get; set; }  // MODIFY
        public bool     IsDeleted      { get; set; }  // ES_DELETE

        public List<ProductionToStoreDetailDto> Details { get; set; } = new();
    }

    public class ProductionToStoreDetailDto
    {
        public int      ProductionCode { get; set; }  // PSD_PS_CODE
        public int?     ItemCode       { get; set; }  // PSD_I_CODE
        public string?  ItemName       { get; set; }  // from ITEM_MASTER
        public decimal  Quantity       { get; set; }  // PSD_QTY
        public string?  Remark         { get; set; }  // PSD_REMARK
    }

    public class CreateProductionToStoreRequest
    {
        public int      CompanyCode   { get; set; }
        public DateTime? GinDate      { get; set; }
        public string?  Type          { get; set; }
        public string?  PersonName    { get; set; }
        public decimal? MrCode        { get; set; }
        public int?     CustomerCode  { get; set; }
        public int?     BatchNo       { get; set; }

        public List<CreateProductionToStoreDetailRequest> Details { get; set; } = new();
    }

    public class CreateProductionToStoreDetailRequest
    {
        public int?    ItemCode   { get; set; }
        public decimal Quantity   { get; set; }
        public string? Remark     { get; set; }
    }

    public class UpdateProductionToStoreRequest : CreateProductionToStoreRequest
    {
        public int ProductionCode { get; set; }
    }

    public class ProductionToStoreQueryParameters : QueryParameters
    {
        public int?      CompanyCode  { get; set; }
        public string?   SearchText   { get; set; }
        public DateTime? DateFrom     { get; set; }
        public DateTime? DateTo       { get; set; }
    }
}
