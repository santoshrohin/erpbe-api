using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Queries
{
    public class GetCustomerMastersQuery : IRequest<PagedResponse<CustomerMasterDto>>
    {
        public CustomerMasterQueryParameters Parameters { get; set; } = new();
    }
}

