using ErpBE.Application.Audit;
using ErpBE.Application.CustomerTypeMaster.Commands;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class CreateCustomerTypeMasterCommandHandler : IRequestHandler<CreateCustomerTypeMasterCommand, CustomerTypeMasterDto>
    {
        private readonly ICustomerTypeMasterRepository _repository;
        private readonly IAuditService                 _auditService;
        private readonly IActivityLogService           _activityLog;
        private readonly ICompanyContext               _ctx;

        public CreateCustomerTypeMasterCommandHandler(
            ICustomerTypeMasterRepository repository,
            IAuditService                 auditService,
            IActivityLogService           activityLog,
            ICompanyContext               ctx)
        {
            _repository   = repository;
            _auditService = auditService;
            _activityLog  = activityLog;
            _ctx          = ctx;
        }

        public async Task<CustomerTypeMasterDto> Handle(CreateCustomerTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId       = request.CompanyId,
                TypeCode        = request.TypeCode.Trim(),
                TypeDescription = request.TypeDescription.Trim(),
                FirstLetter     = request.FirstLetter.Trim()
            };

            var result = await _repository.CreateAsync(createRequest, cancellationToken);

            await _auditService.LogCreateAsync(
                tableName: "CUSTOMER_TYPE_MASTER",
                recordId:  result.Id,
                newValues: result,
                createdBy: "System"
            );

            await _activityLog.WriteLogAsync(
                companyId: request.CompanyId,
                source:    "CustomerTypeMaster",
                @event:    "INSERT",
                docName:   "Customer Type Master",
                docNo:     request.TypeCode,
                docCode:   result.Id,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return result;
        }
    }
}
