using ErpBE.Application.Audit;
using ErpBE.Application.CustomerTypeMaster.Commands;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class CreateCustomerTypeMasterCommandHandler : IRequestHandler<CreateCustomerTypeMasterCommand, CustomerTypeMasterDto>
    {
        private readonly ICustomerTypeMasterRepository _repository;
        private readonly IAuditService _auditService;

        public CreateCustomerTypeMasterCommandHandler(
            ICustomerTypeMasterRepository repository,
            IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        public async Task<CustomerTypeMasterDto> Handle(CreateCustomerTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId = request.CompanyId,
                TypeCode = request.TypeCode.Trim(),
                TypeDescription = request.TypeDescription.Trim(),
                FirstLetter = request.FirstLetter.Trim()
            };

            var result = await _repository.CreateAsync(createRequest, cancellationToken);

            await _auditService.LogCreateAsync(
                tableName: "CUSTOMER_TYPE_MASTER",
                recordId: result.Id,
                newValues: result,
                createdBy: "System"
            );

            return result;
        }
    }
}

