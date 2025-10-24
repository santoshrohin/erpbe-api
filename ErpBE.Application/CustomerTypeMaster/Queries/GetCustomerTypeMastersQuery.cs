using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Queries
{
    public class GetCustomerTypeMastersQuery : IRequest<PagedResponse<CustomerTypeMasterDto>>
    {
        public CustomerTypeMasterQueryParameters Parameters { get; set; } = new();
    }
}

