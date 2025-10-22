namespace ErpBE.Domain.Entities
{
    public class UnitMaster
    {
        public int I_UOM_CODE { get; set; }
        public int I_UOM_CM_COMP_ID { get; set; }
        public string I_UOM_NAME { get; set; } = string.Empty;
        public string I_UOM_DESC { get; set; } = string.Empty;
        public bool ES_DELETE { get; set; }
        public bool MODIFY { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
