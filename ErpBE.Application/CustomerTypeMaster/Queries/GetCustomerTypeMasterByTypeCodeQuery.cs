using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Queries
{
    public class GetCustomerTypeMasterByTypeCodeQuery : IRequest<CustomerTypeMasterDto?>
    {
        public string TypeCode { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }
}

