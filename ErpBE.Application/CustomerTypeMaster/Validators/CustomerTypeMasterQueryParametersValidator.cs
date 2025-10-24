using ErpBE.Application.Common.Models;
using FluentValidation;

namespace ErpBE.Application.CustomerTypeMaster.Validators
{
    public class CustomerTypeMasterQueryParametersValidator : AbstractValidator<CustomerTypeMasterQueryParameters>
    {
        public CustomerTypeMasterQueryParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page Number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page Size must be between 1 and 100.");

            RuleFor(x => x.SortDirection)
                .Must(direction => string.IsNullOrEmpty(direction) || 
                                   direction.Equals("ASC", StringComparison.OrdinalIgnoreCase) ||
                                   direction.Equals("DESC", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Sort Direction must be either 'ASC' or 'DESC'.");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || 
                               IsValidSortColumn(sortBy))
                .WithMessage("Invalid Sort By column. Allowed values: TypeCode, TypeDescription, FirstLetter.");

            When(x => x.CompanyId.HasValue, () =>
            {
                RuleFor(x => x.CompanyId!.Value)
                    .GreaterThan(0)
                    .WithMessage("Company ID must be greater than 0.");
            });

            When(x => !string.IsNullOrEmpty(x.TypeCode), () =>
            {
                RuleFor(x => x.TypeCode)
                    .MaximumLength(50)
                    .WithMessage("Type Code filter cannot exceed 50 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.TypeDescription), () =>
            {
                RuleFor(x => x.TypeDescription)
                    .MaximumLength(150)
                    .WithMessage("Type Description filter cannot exceed 150 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.FirstLetter), () =>
            {
                RuleFor(x => x.FirstLetter)
                    .MaximumLength(50)
                    .WithMessage("First Letter filter cannot exceed 50 characters.");
            });
        }

        private static bool IsValidSortColumn(string sortBy)
        {
            var validColumns = new[] { "TypeCode", "TypeDescription", "FirstLetter" };
            return validColumns.Any(col => col.Equals(sortBy, StringComparison.OrdinalIgnoreCase));
        }
    }
}

