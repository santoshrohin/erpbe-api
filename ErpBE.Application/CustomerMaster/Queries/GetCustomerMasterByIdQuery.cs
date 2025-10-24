using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Queries
{
    public class GetCustomerMasterByIdQuery : IRequest<CustomerMasterDto?>
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
}

