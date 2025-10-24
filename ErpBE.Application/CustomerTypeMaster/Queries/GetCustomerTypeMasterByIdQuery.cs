using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Queries
{
    public class GetCustomerTypeMasterByIdQuery : IRequest<CustomerTypeMasterDto?>
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
}

