using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerPo.Queries
{
    public class GetCustomerPoPrintDataQuery : IRequest<CustomerPoPrintDto?>
    {
        public int PoCode { get; set; }
        public int CompanyId { get; set; }
        public int CompanyCode { get; set; }
        public PoCopyType CopyType { get; set; } = PoCopyType.Original;
    }
}

