using ErpBE.Application.CustomerTypeMaster.Queries;
using FluentValidation;

namespace ErpBE.Application.CustomerTypeMaster.Validators
{
    public class GetCustomerTypeMastersQueryValidator : AbstractValidator<GetCustomerTypeMastersQuery>
    {
        public GetCustomerTypeMastersQueryValidator()
        {
            RuleFor(x => x.Parameters.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page Number must be greater than 0.");

            RuleFor(x => x.Parameters.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page Size must be between 1 and 100.");

            RuleFor(x => x.Parameters.SortDirection)
                .Must(direction => string.IsNullOrEmpty(direction) || 
                                   direction.Equals("ASC", StringComparison.OrdinalIgnoreCase) ||
                                   direction.Equals("DESC", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Sort Direction must be either 'ASC' or 'DESC'.");

            RuleFor(x => x.Parameters.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || 
                               IsValidSortColumn(sortBy))
                .WithMessage("Invalid Sort By column. Allowed values: TypeCode, TypeDescription, FirstLetter.");

            When(x => x.Parameters.CompanyId.HasValue, () =>
            {
                RuleFor(x => x.Parameters.CompanyId!.Value)
                    .GreaterThan(0)
                    .WithMessage("Company ID must be greater than 0.");
            });
        }

        private static bool IsValidSortColumn(string sortBy)
        {
            var validColumns = new[] { "TypeCode", "TypeDescription", "FirstLetter" };
            return validColumns.Any(col => col.Equals(sortBy, StringComparison.OrdinalIgnoreCase));
        }
    }
}

