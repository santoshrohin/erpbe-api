using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class CheckItemCategoryNameUniqueQuery : IRequest<bool>
    {
        public string CategoryName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int? ExcludeCategoryId { get; set; }
    }
}



