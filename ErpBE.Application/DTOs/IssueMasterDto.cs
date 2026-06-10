using ErpBE.Application.Common.Models;

namespace ErpBE.Application.DTOs
{
    public class IssueMasterDto
    {
        public int      IssueCode    { get; set; }  // IM_CODE
        public decimal  IssueNumber  { get; set; }  // IM_NO
        public DateTime IssueDate    { get; set; }  // IM_DATE
        public string?  IssueType    { get; set; }  // IM_TYPE ('FREE' or 'MR')
        public string?  IssuedBy     { get; set; }  // IM_ISSUEBY
        public string?  RequestedBy  { get; set; }  // IM_REQBY
        public int      CompanyCode  { get; set; }  // IM_COMP_ID
        public bool     IsDeleted    { get; set; }  // ES_DELETE

        public List<IssueMasterDetailDto> Details { get; set; } = new();
    }

    public class IssueMasterDetailDto
    {
        public int      IssueCode    { get; set; }  // IM_CODE
        public int?     ItemCode     { get; set; }  // IMD_I_CODE
        public string?  ItemName     { get; set; }
        public int?     UomCode      { get; set; }  // IMD_UOM
        public string?  UomName      { get; set; }
        public decimal  CurrentStock { get; set; }  // IMD_CURR_STOCK
        public decimal  RequestedQty { get; set; }  // IMD_REQ_QTY
        public decimal  IssuedQty    { get; set; }  // IMD_ISSUE_QTY
        public string?  Remark       { get; set; }  // IMD_REMARK
        public decimal? Rate         { get; set; }  // IMD_RATE
        public decimal? Amount       { get; set; }  // IMD_AMOUNT
    }

    public class CreateIssueMasterRequest
    {
        public int        CompanyCode    { get; set; }
        public DateTime?  IssueDate      { get; set; }
        public string?    IssueType      { get; set; }
        public decimal?   MaterialReqNo  { get; set; }
        public string?    IssuedBy       { get; set; }
        public string?    RequestedBy    { get; set; }
        public int?       UserMasterCode { get; set; }
        public int        FromStore      { get; set; } = -2147483647;

        public List<CreateIssueMasterDetailRequest> Details { get; set; } = new();
    }

    public class CreateIssueMasterDetailRequest
    {
        public int?     ItemCode     { get; set; }
        public int?     UomCode      { get; set; }
        public decimal  CurrentStock { get; set; }
        public decimal  RequestedQty { get; set; }
        public decimal  IssuedQty    { get; set; }
        public string?  Remark       { get; set; }
        public decimal? Rate         { get; set; }
        public decimal? Amount       { get; set; }
        public int?     ToStore      { get; set; }
    }

    public class UpdateIssueMasterRequest : CreateIssueMasterRequest
    {
        public int IssueCode { get; set; }
    }

    public class IssueMasterQueryParameters : QueryParameters
    {
        public int?       CompanyCode { get; set; }
        public string?    SearchText  { get; set; }
        public DateTime?  DateFrom    { get; set; }
        public DateTime?  DateTo      { get; set; }
    }
}
