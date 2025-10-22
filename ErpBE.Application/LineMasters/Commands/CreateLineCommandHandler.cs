using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.LineMasters.Commands
{
    public class CreateLineCommandHandler : IRequestHandler<CreateLineCommand, int>
    {
        private readonly ILineRepository _repository;

        public CreateLineCommandHandler(ILineRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateLineCommand request, CancellationToken cancellationToken)
        {
            return await _repository.CreateAsync(request.LineName);
        }
    }

}
