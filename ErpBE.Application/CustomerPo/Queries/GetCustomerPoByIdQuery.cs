using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerPo.Queries;

/// <summary>
/// Query to get a Customer PO by ID with all details
/// </summary>
public class GetCustomerPoByIdQuery : IRequest<CustomerPoMasterDto?>
{
    public int PoCode { get; set; }
    public int CompanyId { get; set; }
}

