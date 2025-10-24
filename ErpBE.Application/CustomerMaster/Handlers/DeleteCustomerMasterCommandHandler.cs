using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class DeleteCustomerMasterCommandHandler : IRequestHandler<DeleteCustomerMasterCommand, Unit>
    {
        private readonly ICustomerMasterRepository _repository;

        public DeleteCustomerMasterCommandHandler(ICustomerMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeleteCustomerMasterCommand request, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(request.Id, request.CompanyId);
            return Unit.Value;
        }
    }
}

