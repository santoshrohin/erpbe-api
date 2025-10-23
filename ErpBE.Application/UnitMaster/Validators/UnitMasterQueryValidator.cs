using FluentValidation;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Validators;

namespace ErpBE.Application.UnitMaster.Validators
{
    public class UnitMasterQueryValidator : AbstractValidator<UnitMasterQueryParameters>
    {
        private static readonly string[] ValidSortFields = { "Id", "UnitName", "UnitDescription", "CompanyId", "IsActive" };

        public UnitMasterQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .ValidPageNumber();

            RuleFor(x => x.PageSize)
                .ValidPageSize(100);

            RuleFor(x => x.CompanyId)
                .GreaterThanZero("Company ID")
                .When(x => x.CompanyId.HasValue);

            RuleFor(x => x.UnitName)
                .MaxLengthNullable(10, "Unit name filter")
                .AlphanumericOnlyNullable("Unit name filter")
                .When(x => !string.IsNullOrEmpty(x.UnitName));

            RuleFor(x => x.SearchTerm)
                .MaxLengthNullable(255, "Search term")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.SortBy)
                .MustBeOneOfNullable(ValidSortFields, "Sort field")
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.SortDirection)
                .ValidSortDirectionNullable()
                .When(x => !string.IsNullOrEmpty(x.SortDirection));
        }
    }
}
