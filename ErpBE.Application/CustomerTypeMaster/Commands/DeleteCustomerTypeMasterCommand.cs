using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Commands
{
    public class DeleteCustomerTypeMasterCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
}

