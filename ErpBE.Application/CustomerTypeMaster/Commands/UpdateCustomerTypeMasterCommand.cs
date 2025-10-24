using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Commands
{
    public class UpdateCustomerTypeMasterCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string TypeCode { get; set; } = string.Empty;
        public string TypeDescription { get; set; } = string.Empty;
        public string FirstLetter { get; set; } = string.Empty;
    }
}

