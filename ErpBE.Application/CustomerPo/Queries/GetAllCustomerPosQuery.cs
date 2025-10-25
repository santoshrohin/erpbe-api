using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerPo.Queries;

/// <summary>
/// Query to get all Customer POs with pagination, filtering, and sorting
/// </summary>
public class GetAllCustomerPosQuery : IRequest<(IEnumerable<CustomerPoMasterDto> Data, int TotalCount)>
{
    public CustomerPoQueryParameters Parameters { get; set; } = new();
}

