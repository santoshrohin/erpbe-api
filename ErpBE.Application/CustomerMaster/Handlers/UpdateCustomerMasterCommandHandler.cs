using ErpBE.Application.Audit;
using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class UpdateCustomerMasterCommandHandler : IRequestHandler<UpdateCustomerMasterCommand, Unit>
    {
        private readonly ICustomerMasterRepository _repository;
        private readonly IAuditService _auditService;

        public UpdateCustomerMasterCommandHandler(ICustomerMasterRepository repository, IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        public async Task<Unit> Handle(UpdateCustomerMasterCommand request, CancellationToken cancellationToken)
        {
            // Check if record exists
            var existingRecord = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (existingRecord == null)
            {
                throw new KeyNotFoundException($"Customer Master with ID '{request.Id}' not found.");
            }

            // Check if modified by another user
            if (await _repository.IsModifiedByAnotherUserAsync(request.Id, cancellationToken))
            {
                throw new InvalidOperationException("This record is currently being modified by another user. Please try again later.");
            }

            // Trim all inputs
            var updateRequest = new UpdateCustomerMasterRequest
            {
                Id = request.Id,
                CompanyId = request.CompanyId,
                PartyCode = request.PartyCode.Trim(),
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

            // Business Logic: Check uniqueness of Party Name (case-insensitive, excluding current record)
            if (!await _repository.IsPartyNameUniqueAsync(updateRequest.PartyName, request.Id, updateRequest.CompanyId, cancellationToken))
            {
                throw new InvalidOperationException("Record Already Exists");
            }

            // Business Logic: Check uniqueness of Abbreviation if provided (case-insensitive, excluding current record)
            if (!string.IsNullOrWhiteSpace(updateRequest.Abbreviation))
            {
                if (!await _repository.IsAbbreviationUniqueAsync(updateRequest.Abbreviation, request.Id, updateRequest.CompanyId, cancellationToken))
                {
                    throw new InvalidOperationException("Abbrevation Already Exists");
                }
            }

            var success = await _repository.UpdateAsync(updateRequest, cancellationToken);

            if (!success)
            {
                throw new InvalidOperationException("Failed to update Customer Master.");
            }

            await _auditService.LogUpdateAsync(
                tableName: "PARTY_MASTER",
                recordId: request.Id,
                oldValues: existingRecord,
                newValues: updateRequest,
                modifiedBy: "System"
            );

            return Unit.Value;
        }
    }
}

