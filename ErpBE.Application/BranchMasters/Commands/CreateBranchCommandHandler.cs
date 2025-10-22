using ErpBE.Domain.FarmerEntities;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.BranchMasters.Commands
{
    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, int>
    {
        private readonly IBranchRepository _repository;

        public CreateBranchCommandHandler(IBranchRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreateAsync(request.BranchName);
        }
    }

}
