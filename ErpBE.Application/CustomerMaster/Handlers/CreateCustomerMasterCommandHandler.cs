using ErpBE.Application.Audit;
using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class CreateCustomerMasterCommandHandler : IRequestHandler<CreateCustomerMasterCommand, CustomerMasterDto>
    {
        private readonly ICustomerMasterRepository _repository;
        private readonly IAuditService _auditService;

        public CreateCustomerMasterCommandHandler(ICustomerMasterRepository repository, IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        public async Task<CustomerMasterDto> Handle(CreateCustomerMasterCommand request, CancellationToken cancellationToken)
        {
            // Trim all inputs
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = request.CompanyId,
                PartyName = request.PartyName.Trim(),
                ContactPerson = request.ContactPerson?.Trim(),
                Abbreviation = request.Abbreviation?.Trim().ToUpper(), // Legacy converts to uppercase
                Address = request.Address?.Trim(),
                Phone = request.Phone?.Trim(),
                Mobile = request.Mobile?.Trim(),
                Email = request.Email?.Trim(),
                Website = request.Website?.Trim(),
                FaxNo = request.FaxNo?.Trim(),
                AreaCode = request.AreaCode,
                CustomerType = request.CustomerType,
                CountryCode = request.CountryCode,
                StateCode = request.StateCode,
                CityCode = request.CityCode,
                PinCode = request.PinCode?.Trim(),
                VatTinNo = request.VatTinNo?.Trim(),
                CstNo = request.CstNo?.Trim(),
                GstNo = request.GstNo?.Trim(),
                PanNo = request.PanNo?.Trim(),
                ServiceTaxNo = request.ServiceTaxNo?.Trim(),
                TallyName = request.TallyName?.Trim(),
                OpeningBalance = request.OpeningBalance,
                OpeningBalanceType = request.OpeningBalanceType?.Trim(),
                CreditLimit = request.CreditLimit,
                CreditDays = request.CreditDays,
                BankName = request.BankName?.Trim(),
                BankAccountNo = request.BankAccountNo?.Trim(),
                BankBranchName = request.BankBranchName?.Trim(),
                BankIfscCode = request.BankIfscCode?.Trim(),
                IsLbtApplicable = request.IsLbtApplicable,
                IsSezCustomer = request.IsSezCustomer,
                IsCompositeDealer = request.IsCompositeDealer,
                Remark = request.Remark?.Trim()
            };

            // Business Logic: Check uniqueness of Party Name (case-insensitive)
            if (!await _repository.IsPartyNameUniqueAsync(createRequest.PartyName, null, createRequest.CompanyId, cancellationToken))
            {
                throw new InvalidOperationException("Record Already Exists");
            }

            // Business Logic: Check uniqueness of Abbreviation if provided (case-insensitive)
            if (!string.IsNullOrWhiteSpace(createRequest.Abbreviation))
            {
                if (!await _repository.IsAbbreviationUniqueAsync(createRequest.Abbreviation, null, createRequest.CompanyId, cancellationToken))
                {
                    throw new InvalidOperationException("Abbrevation Already Exists");
                }
            }

            var result = await _repository.CreateAsync(createRequest, cancellationToken);

            await _auditService.LogCreateAsync(
                tableName: "PARTY_MASTER",
                recordId: result.Id,
                newValues: result,
                createdBy: "System"
            );

            return result;
        }
    }
}

