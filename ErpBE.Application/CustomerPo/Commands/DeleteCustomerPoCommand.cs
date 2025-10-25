using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

/// <summary>
/// Command to delete (soft delete) a Customer PO
/// </summary>
public class DeleteCustomerPoCommand : IRequest<bool>
{
    public int PoCode { get; set; }
    public int CompanyId { get; set; }
}

