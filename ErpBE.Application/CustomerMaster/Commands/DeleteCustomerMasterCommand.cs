using MediatR;

namespace ErpBE.Application.CustomerMaster.Commands
{
    public class DeleteCustomerMasterCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
}

