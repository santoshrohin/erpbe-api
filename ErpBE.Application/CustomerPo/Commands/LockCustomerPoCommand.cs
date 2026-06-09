using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

public class LockCustomerPoCommand : IRequest<bool>
{
    public int PoCode    { get; set; }
    public int CompanyId { get; set; }
}
