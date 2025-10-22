using ErpBE.Domain.CommonDto;

namespace ErpBE.Domain.DTOs
{
    public class UnitMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ModifiedBy { get; set; }
    }

    public class CreateUnitMasterRequest
    {
        public string UnitName { get; set; } = string.Empty;
        public string UnitDescription { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUnitMasterRequest
    {
        public int Id { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string UnitDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UnitMasterQueryParameters : QueryParameters
    {
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public string? UnitName { get; set; }
    }
}
