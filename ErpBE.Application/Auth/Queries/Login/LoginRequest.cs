using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ErpBE.Application.Auth.Queries.Login
{
    public class LoginRequest : IRequest<LoginResponse>
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "CompanyId is required")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "FinancialYearCode is required")]
        public int FinancialYearCode { get; set; }
    }
}
